using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class OpeningScheduleService : IOpeningScheduleService
{
    // private readonly IOpeningScheduleRepository _openingScheduleRepository;
    //
    // public OpeningScheduleService(IOpeningScheduleRepository openingScheduleRepository) => _openingScheduleRepository = openingScheduleRepository;

    private readonly IUnitOfWork _unitOfWork;

    public OpeningScheduleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<OpeningScheduleDto>> GetAllAsync()
    {
        var schedules = await _unitOfWork.OpeningSchedules.GetAllAsync();
        return schedules.Select(MapToOpeningScheduleDto);
    }

    public async Task<OpeningScheduleDto> GetByIdAsync(int id)
    {
        var schedule = await _unitOfWork.OpeningSchedules.GetByIdAsync(id);
        if (schedule == null)
        {
            throw new NotFoundException("Opening Schedule", "Id", id);
        }

        return MapToOpeningScheduleDto(schedule);
    }

    public async Task<OpeningScheduleDto> AddAsync(CreateOpeningScheduleDto createOpeningScheduleDto)
    {
        // Thêm kiểm tra logic: ví dụ, ClassCode có thể cần là duy nhất
        var existingScheduleWithClassCode =
            await _unitOfWork.OpeningSchedules.FindOneAsync(os =>
                os.class_code == createOpeningScheduleDto.ClassCode); // Giả định FindOneAsync có sẵn
        if (existingScheduleWithClassCode != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "ClassCode", new string[] { $"Mã lớp '{createOpeningScheduleDto.ClassCode}' đã tồn tại." } }
            });
        }

        // Kiểm tra logic: Ngày kết thúc không được trước ngày khai giảng
        if (createOpeningScheduleDto.EndDate.HasValue &&
            createOpeningScheduleDto.EndDate.Value < createOpeningScheduleDto.OpeningDay)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "EndDate", new string[] { "Ngày kết thúc không được trước ngày khai giảng." } }
            });
        }

        var scheduleEntity = new opening_schedule
        {
            subject = createOpeningScheduleDto.Subject,
            class_code = createOpeningScheduleDto.ClassCode,
            opening_day = createOpeningScheduleDto.OpeningDay,
            end_date = createOpeningScheduleDto.EndDate,
            schedule = createOpeningScheduleDto.Schedule,
            student_quantity = createOpeningScheduleDto.StudentQuantity,
            is_advanced_class = createOpeningScheduleDto.IsAdvancedClass ?? false
        };

        try
        {
            var addedSchedule = await _unitOfWork.OpeningSchedules.AddAsync(scheduleEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
            return MapToOpeningScheduleDto(addedSchedule);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm lịch khai giảng vào cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the opening schedule.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    // UPDATE Opening Schedule
    public async Task UpdateAsync(UpdateOpeningScheduleDto updateOpeningScheduleDto)
    {
        var existingSchedule =
            await _unitOfWork.OpeningSchedules.GetByIdAsync(updateOpeningScheduleDto.OpeningScheduleId);

        if (existingSchedule == null)
        {
            throw new NotFoundException("Opening Schedule", "Id", updateOpeningScheduleDto.OpeningScheduleId);
        }

        // Kiểm tra logic: ClassCode có thể cần là duy nhất khi cập nhật
        if (!string.IsNullOrEmpty(updateOpeningScheduleDto.ClassCode) &&
            updateOpeningScheduleDto.ClassCode != existingSchedule.class_code)
        {
            var scheduleWithSameClassCode =
                await _unitOfWork.OpeningSchedules.FindOneAsync(os =>
                    os.class_code == updateOpeningScheduleDto.ClassCode);
            if (scheduleWithSameClassCode != null && scheduleWithSameClassCode.opening_schedule_id !=
                updateOpeningScheduleDto.OpeningScheduleId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    {
                        "ClassCode",
                        new string[]
                        {
                            $"Mã lớp '{updateOpeningScheduleDto.ClassCode}' đã được sử dụng bởi một lịch khai giảng khác."
                        }
                    }
                });
            }
        }

        // Kiểm tra logic: Ngày kết thúc không được trước ngày khai giảng (nếu cả hai đều được cung cấp hoặc thay đổi)
        DateOnly effectiveOpeningDay =
            updateOpeningScheduleDto.OpeningDay ??
            existingSchedule.opening_day ?? default; // Lấy giá trị hiện tại nếu không thay đổi
        DateOnly effectiveEndDate =
            updateOpeningScheduleDto.EndDate ??
            existingSchedule.end_date ?? default; // Lấy giá trị hiện tại nếu không thay đổi

        if (updateOpeningScheduleDto.EndDate.HasValue && updateOpeningScheduleDto.EndDate.Value < effectiveOpeningDay)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "EndDate", new string[] { "Ngày kết thúc không được trước ngày khai giảng." } }
            });
        }

        // Hoặc nếu chỉ OpeningDay được cập nhật và nó đẩy lùi sau EndDate cũ
        if (updateOpeningScheduleDto.OpeningDay.HasValue && existingSchedule.end_date.HasValue &&
            updateOpeningScheduleDto.OpeningDay.Value > existingSchedule.end_date.Value)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "OpeningDay", new string[] { "Ngày khai giảng không được sau ngày kết thúc hiện tại." } }
            });
        }


        // Cập nhật các trường nếu có giá trị được cung cấp (dùng toán tử null-coalescing assignment ??= nếu muốn)
        existingSchedule.subject = updateOpeningScheduleDto.Subject ?? existingSchedule.subject;
        existingSchedule.class_code = updateOpeningScheduleDto.ClassCode ?? existingSchedule.class_code;
        existingSchedule.opening_day = updateOpeningScheduleDto.OpeningDay ?? existingSchedule.opening_day;
        existingSchedule.end_date = updateOpeningScheduleDto.EndDate ?? existingSchedule.end_date;
        existingSchedule.schedule = updateOpeningScheduleDto.Schedule ?? existingSchedule.schedule;
        existingSchedule.student_quantity =
            updateOpeningScheduleDto.StudentQuantity ?? existingSchedule.student_quantity;
        existingSchedule.is_advanced_class =
            updateOpeningScheduleDto.IsAdvancedClass ?? existingSchedule.is_advanced_class;


        try
        {
            await _unitOfWork.OpeningSchedules.UpdateAsync(existingSchedule);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật lịch khai giảng trong cơ sở dữ liệu.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the opening schedule.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var scheduleToDelete = await _unitOfWork.OpeningSchedules.GetByIdAsync(id);
        if (scheduleToDelete == null)
        {
            throw new NotFoundException("Opening Schedule", "Id", id);
        }

        try
        {
            await _unitOfWork.OpeningSchedules.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu có user nào đó đang liên kết với lịch khai giảng này, sẽ ném lỗi FK
            throw new ApiException("Không thể xóa lịch khai giảng này vì nó đang được sử dụng bởi một hoặc nhiều người dùng.", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the opening schedule.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<OpeningScheduleDto>> SearchOpeningSchedulesAsync(string? subject = null,
        string? classCode = null, DateOnly? openingDay = null,
        DateOnly? endDate = null, string? schedule = null, int? studentQuantity = null, bool? isAdvancedClass = null)
    {
        var schedules = await _unitOfWork.OpeningSchedules.SearchOpeningSchedulesAsync(
            subject, classCode, openingDay, endDate, schedule, studentQuantity, isAdvancedClass);
        // Map kết quả tìm kiếm sang DTO
        return schedules.Select(MapToOpeningScheduleDto);
    }

    private OpeningScheduleDto MapToOpeningScheduleDto(opening_schedule model)
    {
        return new OpeningScheduleDto
        {
            OpeningScheduleId = model.opening_schedule_id,
            Subject = model.subject,
            ClassCode = model.class_code,
            OpeningDay = model.opening_day,
            EndDate = model.end_date,
            Schedule = model.schedule,
            StudentQuantity = model.student_quantity,
            IsAdvancedClass = model.is_advanced_class
        };
    }
}