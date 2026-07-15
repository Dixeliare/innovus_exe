using DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Repository.Basic.IRepositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;
using Services.Services;

namespace Tests;

[TestClass]
public class ScheduleServiceTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IScheduleRepository> _scheduleRepoMock = null!;
    private Mock<IWeekService> _weekServiceMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<ILogger<ScheduleService>> _loggerMock = null!;
    private ScheduleService _sut = null!; // system under test

    [TestInitialize]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _scheduleRepoMock = new Mock<IScheduleRepository>();
        _weekServiceMock = new Mock<IWeekService>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<ScheduleService>>();

        // IUnitOfWork.Schedules trả về repository đã mock ở trên
        _unitOfWorkMock.Setup(u => u.Schedules).Returns(_scheduleRepoMock.Object);

        _sut = new ScheduleService(
            _unitOfWorkMock.Object,
            _weekServiceMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task AddAsync_ShouldThrowValidationException_WhenMonthYearIsNull()
    {
        // Arrange: MonthYear để null -> phải bị chặn ngay từ đầu, không được đụng tới DB
        var dto = new CreateScheduleDto
        {
            MonthYear = null,
            Note = "Test note"
        };

        // Act + Assert (dùng try/catch thay vì Assert.ThrowsExceptionAsync để né lỗi overload của MSTest 10.0)
        try
        {
            await _sut.AddAsync(dto);
            Assert.Fail("Expected ValidationException was not thrown.");
        }
        catch (ValidationException)
        {
            // đúng như kỳ vọng
        }

        // Đảm bảo không hề gọi xuống repository khi validate đã fail sớm
        _scheduleRepoMock.Verify(r => r.GetByMonthYearExactAsync(It.IsAny<DateOnly>()), Times.Never);
    }

    [TestMethod]
    public async Task AddAsync_ShouldThrowApiException_WhenScheduleForMonthYearAlreadyExists()
    {
        // Arrange
        var monthYear = new DateOnly(2026, 8, 1);
        var dto = new CreateScheduleDto { MonthYear = monthYear, Note = "August" };

        var existing = new schedule { schedule_id = 1, month_year = monthYear };
        _scheduleRepoMock
            .Setup(r => r.GetByMonthYearExactAsync(monthYear))
            .ReturnsAsync(existing);

        // Act
        ApiException? ex = null;
        try
        {
            await _sut.AddAsync(dto);
            Assert.Fail("Expected ApiException was not thrown.");
        }
        catch (ApiException e)
        {
            ex = e;
        }

        // Assert: đúng status code Conflict (409) như code thật trả về
        Assert.IsNotNull(ex);
        Assert.AreEqual(409, ex!.StatusCode);
        _scheduleRepoMock.Verify(r => r.AddAsync(It.IsAny<schedule>()), Times.Never);
    }

    [TestMethod]
    public async Task AddAsync_ShouldCreateScheduleAndGenerateWeeks_WhenValidAndNotDuplicate()
    {
        // Arrange
        var monthYear = new DateOnly(2026, 9, 1);
        var dto = new CreateScheduleDto { MonthYear = monthYear, Note = "September" };

        _scheduleRepoMock
            .Setup(r => r.GetByMonthYearExactAsync(monthYear))
            .ReturnsAsync((schedule?)null); // chưa tồn tại -> hợp lệ

        var savedSchedule = new schedule { schedule_id = 42, month_year = monthYear, note = dto.Note };
        _scheduleRepoMock
            .Setup(r => r.AddAsync(It.IsAny<schedule>()))
            .ReturnsAsync(savedSchedule);

        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        // Act
        var result = await _sut.AddAsync(dto);

        // Assert
        Assert.AreEqual(42, result.ScheduleId);
        Assert.AreEqual(monthYear, result.MonthYear);

        // Verify GenerateWeeksForMonthAsync được gọi đúng với schedule_id, year, month vừa tạo
        _weekServiceMock.Verify(w => w.GenerateWeeksForMonthAsync(
            savedSchedule.schedule_id,
            monthYear.Year,
            monthYear.Month), Times.Once);

        // CompleteAsync phải được gọi 2 lần: 1 lần lưu schedule, 1 lần lưu weeks/days
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Exactly(2));
    }

    [TestMethod]
    public async Task GetByIDAsync_ShouldThrowNotFoundException_WhenScheduleDoesNotExist()
    {
        // Arrange
        _scheduleRepoMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((schedule?)null);

        // Act + Assert
        try
        {
            await _sut.GetByIDAsync(999);
            Assert.Fail("Expected NotFoundException was not thrown.");
        }
        catch (NotFoundException)
        {
            // đúng như kỳ vọng
        }
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDeleteWeeksBeforeDeletingSchedule()
    {
        // Arrange: đảm bảo thứ tự nghiệp vụ đúng - xóa Weeks liên quan TRƯỚC khi xóa Schedule
        var existing = new schedule { schedule_id = 7, month_year = new DateOnly(2026, 5, 1) };
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(existing);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var callOrder = new List<string>();
        _weekServiceMock
            .Setup(w => w.DeleteWeeksByScheduleIdAsync(7))
            .Callback(() => callOrder.Add("DeleteWeeks"))
            .Returns(Task.CompletedTask);
        _scheduleRepoMock
            .Setup(r => r.DeleteAsync(7))
            .Callback(() => callOrder.Add("DeleteSchedule"))
            .ReturnsAsync(true);

        // Act
        await _sut.DeleteAsync(7);

        // Assert
        CollectionAssert.AreEqual(new[] { "DeleteWeeks", "DeleteSchedule" }, callOrder);
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenScheduleDoesNotExist()
    {
        // Arrange
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((schedule?)null);

        // Act + Assert
        try
        {
            await _sut.DeleteAsync(999);
            Assert.Fail("Expected NotFoundException was not thrown.");
        }
        catch (NotFoundException)
        {
            // đúng như kỳ vọng
        }

        // Không được đụng tới weekService hay xóa gì cả khi không tìm thấy schedule
        _weekServiceMock.Verify(w => w.DeleteWeeksByScheduleIdAsync(It.IsAny<int>()), Times.Never);
        _scheduleRepoMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenScheduleDoesNotExist()
    {
        // Arrange
        var dto = new UpdateScheduleDto { ScheduleId = 999, MonthYear = new DateOnly(2026, 10, 1), Note = "x" };
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((schedule?)null);

        // Act + Assert
        try
        {
            await _sut.UpdateAsync(dto);
            Assert.Fail("Expected NotFoundException was not thrown.");
        }
        catch (NotFoundException)
        {
            // đúng như kỳ vọng
        }
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowValidationException_WhenMonthYearIsNull()
    {
        // Arrange: schedule tồn tại, nhưng MonthYear gửi lên bị null
        var existing = new schedule { schedule_id = 5, month_year = new DateOnly(2026, 6, 1) };
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existing);

        var dto = new UpdateScheduleDto { ScheduleId = 5, MonthYear = null, Note = "x" };

        // Act + Assert
        try
        {
            await _sut.UpdateAsync(dto);
            Assert.Fail("Expected ValidationException was not thrown.");
        }
        catch (ValidationException)
        {
            // đúng như kỳ vọng
        }

        // Không được lưu gì cả khi validate fail
        _scheduleRepoMock.Verify(r => r.UpdateAsync(It.IsAny<schedule>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldThrowApiException_WhenNewMonthYearBelongsToAnotherSchedule()
    {
        // Arrange: đang sửa schedule_id = 5 sang tháng 8/2026,
        // nhưng tháng 8/2026 đã bị schedule_id = 8 (KHÁC) chiếm mất
        var newMonthYear = new DateOnly(2026, 8, 1);
        var existing = new schedule { schedule_id = 5, month_year = new DateOnly(2026, 6, 1) };
        var conflictingSchedule = new schedule { schedule_id = 8, month_year = newMonthYear };

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existing);
        _scheduleRepoMock
            .Setup(r => r.GetByMonthYearExactAsync(newMonthYear))
            .ReturnsAsync(conflictingSchedule);

        var dto = new UpdateScheduleDto { ScheduleId = 5, MonthYear = newMonthYear, Note = "x" };

        // Act
        ApiException? ex = null;
        try
        {
            await _sut.UpdateAsync(dto);
            Assert.Fail("Expected ApiException was not thrown.");
        }
        catch (ApiException e)
        {
            ex = e;
        }

        // Assert
        Assert.IsNotNull(ex);
        Assert.AreEqual(409, ex!.StatusCode);
        _scheduleRepoMock.Verify(r => r.UpdateAsync(It.IsAny<schedule>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldAllowKeepingSameMonthYear_OnSameSchedule()
    {
        // Arrange: schedule_id = 5 đang là tháng 6/2026, update NHƯNG giữ nguyên tháng 6/2026
        // -> existingSchedule.month_year == updateScheduleDto.MonthYear.Value, nên code SẼ KHÔNG check trùng,
        // không được throw Conflict với chính bản thân nó.
        var monthYear = new DateOnly(2026, 6, 1);
        var existing = new schedule { schedule_id = 5, month_year = monthYear, note = "old note" };
        _scheduleRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existing);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var dto = new UpdateScheduleDto { ScheduleId = 5, MonthYear = monthYear, Note = "new note" };

        // Act
        await _sut.UpdateAsync(dto);

        // Assert: không được gọi kiểm tra trùng vì tháng/năm không đổi
        _scheduleRepoMock.Verify(r => r.GetByMonthYearExactAsync(It.IsAny<DateOnly>()), Times.Never);
        _scheduleRepoMock.Verify(r => r.UpdateAsync(It.Is<schedule>(s => s.note == "new note")), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ShouldUpdateSuccessfully_WhenValidAndNoConflict()
    {
        // Arrange: đổi từ tháng 6/2026 sang tháng 11/2026, chưa ai chiếm tháng 11/2026
        var oldMonthYear = new DateOnly(2026, 6, 1);
        var newMonthYear = new DateOnly(2026, 11, 1);
        var existing = new schedule { schedule_id = 5, month_year = oldMonthYear, note = "old note" };

        _scheduleRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existing);
        _scheduleRepoMock
            .Setup(r => r.GetByMonthYearExactAsync(newMonthYear))
            .ReturnsAsync((schedule?)null);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var dto = new UpdateScheduleDto { ScheduleId = 5, MonthYear = newMonthYear, Note = "new note" };

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        _scheduleRepoMock.Verify(r => r.UpdateAsync(It.Is<schedule>(
            s => s.month_year == newMonthYear && s.note == "new note")), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}