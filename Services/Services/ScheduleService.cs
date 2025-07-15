using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Data;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class ScheduleService : IScheduleService
{
    // private readonly IScheduleRepository _scheduleRepository;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IWeekService _weekService;

    // Số tháng lịch trình cần duy trì trong tương lai
    private const int MONTHS_TO_ENSURE_IN_FUTURE = 6;

    // Số tháng cũ cần giữ lại (ví dụ: 3 tháng cũ + tháng hiện tại)
    private const int MONTHS_TO_KEEP_OLD = 3;


    // public ScheduleService(IScheduleRepository scheduleRepository) => _scheduleRepository = scheduleRepository;

    public ScheduleService(IUnitOfWork unitOfWork, IWeekService weekService)
    {
        _unitOfWork = unitOfWork;
        _weekService = weekService; // GÁN
    }

    public async Task<IEnumerable<ScheduleDto>> GetAllAsync()
    {
        var schedules = await _unitOfWork.Schedules.GetAllAsync();
        return schedules.Select(MapToScheduleDto);
    }

    public async Task<ScheduleDto> GetByIDAsync(int id)
    {
        var schedule = await _unitOfWork.Schedules.GetByIDAsync(id);
        if (schedule == null)
        {
            throw new NotFoundException("Schedule", "Id", id);
        }
        return MapToScheduleDto(schedule);
    }

    public async Task<IEnumerable<ScheduleDto>> SearchByIdOrNoteAsync(int? id, string? note)
    {
        var schedules = await _unitOfWork.Schedules.SearchByIdOrNoteAsync(id, note);
        return schedules.Select(MapToScheduleDto);
    }

    public async Task<IEnumerable<ScheduleDto>> SearchByMonthYearAsync(int? month, int? year)
    {
        var schedules = await _unitOfWork.Schedules.SearchByMonthYearAsync(month, year);
        return schedules.Select(MapToScheduleDto);
    }

    public async Task<ScheduleDto> AddAsync(CreateScheduleDto createScheduleDto)
    {
        // Có thể thêm validation logic ở đây, ví dụ: không cho phép tạo lịch trình trùng lặp cho cùng một MonthYear
        if (createScheduleDto.MonthYear.HasValue)
        {
            var existing = await _unitOfWork.Schedules.SearchByMonthYearAsync(
                createScheduleDto.MonthYear.Value.Month,
                createScheduleDto.MonthYear.Value.Year
            );
            if (existing != null && existing.Any())
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "MonthYear", new string[] { $"Lịch trình cho tháng {createScheduleDto.MonthYear.Value.Month}/{createScheduleDto.MonthYear.Value.Year} đã tồn tại." } }
                });
            }
        }


        var scheduleEntity = new schedule
        {
            month_year = createScheduleDto.MonthYear,
            note = createScheduleDto.Note,
        };

        try
        {
            var addedSchedule = await _unitOfWork.Schedules.AddAsync(scheduleEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
            return MapToScheduleDto(addedSchedule);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm lịch trình vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the schedule.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // UPDATE Schedule
    public async Task UpdateAsync(UpdateScheduleDto updateScheduleDto)
    {
        var existingSchedule = await _unitOfWork.Schedules.GetByIDAsync(updateScheduleDto.ScheduleId);

        if (existingSchedule == null)
        {
            throw new NotFoundException("Schedule", "Id", updateScheduleDto.ScheduleId);
        }

        // Kiểm tra trùng lặp nếu MonthYear được cập nhật
        if (updateScheduleDto.MonthYear.HasValue && existingSchedule.month_year != updateScheduleDto.MonthYear.Value)
        {
            var existing = await _unitOfWork.Schedules.SearchByMonthYearAsync(
                updateScheduleDto.MonthYear.Value.Month,
                updateScheduleDto.MonthYear.Value.Year
            );
            if (existing != null && existing.Any(s => s.schedule_id != updateScheduleDto.ScheduleId))
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "MonthYear", new string[] { $"Lịch trình cho tháng {updateScheduleDto.MonthYear.Value.Month}/{updateScheduleDto.MonthYear.Value.Year} đã tồn tại với ID khác." } }
                });
            }
        }

        // Cập nhật các trường nếu có giá trị được cung cấp
        if (updateScheduleDto.MonthYear.HasValue)
        {
            existingSchedule.month_year = updateScheduleDto.MonthYear.Value;
        }

        // Nếu Note có thể được set thành null (như trong model), gán trực tiếp
        existingSchedule.note = updateScheduleDto.Note;


        try
        {
            await _unitOfWork.Schedules.UpdateAsync(existingSchedule);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật lịch trình trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the schedule.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int scheduleId)
    {
        var scheduleToDelete = await _unitOfWork.Schedules.GetByIDAsync(scheduleId);
        if (scheduleToDelete == null)
        {
            throw new NotFoundException("Schedule", "Id", scheduleId);
        }

        try
        {
            await _unitOfWork.Schedules.DeleteAsync(scheduleId);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Không thể xóa lịch trình do lỗi cơ sở dữ liệu (ví dụ: đang được tham chiếu bởi các bảng khác).", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the schedule.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }


    // ---------- Logic Chính cho việc tạo và dọn dẹp Schedule tự động ----------

    public async Task EnsureScheduleExistenceAndCleanupAsync()
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateOnly(now.Year, now.Month, 1);

        Console.WriteLine(
            $"[ScheduleService] Running EnsureScheduleExistenceAndCleanupAsync at {now:yyyy-MM-dd HH:mm:ss} UTC");

        try
        {
            await EnsureFutureSchedulesInternalAsync(currentMonthStart);
            await EnsureWeeksForExistingSchedulesAsync(); 
            await CleanupOldSchedulesInternalAsync(currentMonthStart);

            // Đây là nơi quan trọng: Commit TẤT CẢ các thay đổi của unit of work
            // Entity Framework Core sẽ tự động tạo một giao dịch cho SaveChangesAsync
            // nếu chưa có, và rollback nếu có lỗi.
            await _unitOfWork.CompleteAsync();

            Console.WriteLine($"[ScheduleService] Finished EnsureScheduleExistenceAndCleanupAsync successfully.");
        }
        catch (DbUpdateException dbEx)
        {
            // EF Core sẽ tự động rollback giao dịch khi CompleteAsync() thất bại
            throw new ApiException("Database error during schedule management cleanup/creation. Changes have been rolled back.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            // EF Core sẽ tự động rollback giao dịch khi CompleteAsync() thất bại
            throw new ApiException("An unexpected error occurred during schedule management cleanup/creation. Changes have been rolled back.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    private async Task EnsureFutureSchedulesInternalAsync(DateOnly currentMonthStart)
    {
        Console.WriteLine($"[ScheduleService] Ensuring schedules for next {MONTHS_TO_ENSURE_IN_FUTURE} months...");

        var schedulesToAdd = new List<schedule>();
        var allExistingSchedules = (await _unitOfWork.Schedules.GetAllAsync()).ToList();

        for (int i = 0; i < MONTHS_TO_ENSURE_IN_FUTURE; i++)
        {
            var targetDate = currentMonthStart.AddMonths(i);
            var targetMonthYear = new DateOnly(targetDate.Year, targetDate.Month, 1);

            var exists = allExistingSchedules.Any(s =>
                s.month_year.HasValue && s.month_year.Value.Year == targetMonthYear.Year && s.month_year.Value.Month == targetMonthYear.Month
            );

            if (!exists)
            {
                schedulesToAdd.Add(new schedule
                {
                    month_year = targetMonthYear,
                    note = $"Lịch trình tháng {targetMonthYear.Month}/{targetMonthYear.Year}"
                });
                Console.WriteLine(
                    $"[ScheduleService] - Preparing to create schedule for {targetMonthYear.Month}/{targetMonthYear.Year}.");
            }
        }

        if (schedulesToAdd.Any())
        {
            foreach (var s in schedulesToAdd)
            {
                await _unitOfWork.Schedules.AddAsync(s);
                if (s.schedule_id > 0) // Kiểm tra an toàn
                {
                    await GenerateWeeksForMonthAsync(s.schedule_id, s.month_year.Value);
                }
                else
                {
                    Console.WriteLine($"[ScheduleService] - WARNING: schedule_id for {s.month_year.Value} is 0. Weeks will not be generated immediately. This might require calling CompleteAsync() earlier or restructuring.");
                }
            }
            Console.WriteLine($"[ScheduleService] - Created {schedulesToAdd.Count} new schedules.");
        }
        else
        {
            Console.WriteLine("[ScheduleService] - All future schedules already exist.");
        }
    }

    private async Task CleanupOldSchedulesInternalAsync(DateOnly currentMonthStart)
    {
        Console.WriteLine($"[ScheduleService] Cleaning up schedules older than {MONTHS_TO_KEEP_OLD} months...");

        var cutoffDate = currentMonthStart.AddMonths(-MONTHS_TO_KEEP_OLD);

        var oldSchedules = (await _unitOfWork.Schedules.GetAllAsync())
            .Where(s => s.month_year.HasValue && s.month_year.Value < cutoffDate)
            .ToList();

        if (oldSchedules.Any())
        {
            Console.WriteLine(
                $"[ScheduleService] - Found {oldSchedules.Count} old schedules to delete before {cutoffDate.Month}/{cutoffDate.Year}.");
            foreach (var s in oldSchedules)
            {
                // Khi xóa schedule, các week liên quan sẽ bị xóa theo cascade (nếu DB cấu hình đúng)
                // HOẶC bạn cần tự xóa các week trước, nếu không có cascade:
                var weeksToDelete = await _unitOfWork.Weeks.GetWeeksByScheduleIdAsync(s.schedule_id);
                foreach (var week in weeksToDelete)
                {
                    await _unitOfWork.Weeks.DeleteAsync(week.week_id);
                }
                await _unitOfWork.Schedules.DeleteAsync(s.schedule_id);
            }
            Console.WriteLine($"[ScheduleService] - Deleted {oldSchedules.Count} old schedules.");
        }
        else
        {
            Console.WriteLine("[ScheduleService] - No old schedules to delete.");
        }
    }
    
    // PHƯƠNG THỨC MỚI ĐỂ TẠO WEEKS CHO MỘT THÁNG CỤ THỂ
    private async Task GenerateWeeksForMonthAsync(int scheduleId, DateOnly monthYear)
    {
        Console.WriteLine($"[ScheduleService] - Generating weeks for schedule ID {scheduleId} ({monthYear.Month}/{monthYear.Year})...");

        var startOfMonth = new DateOnly(monthYear.Year, monthYear.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); // Ngày cuối cùng của tháng

        int currentWeekNumber = 1; // Bắt đầu từ tuần 1 cho mỗi tháng
        DateOnly currentDay = startOfMonth;

        while (currentDay <= endOfMonth)
        {
            // Tính toán week_number. Ví dụ đơn giản: cứ 7 ngày một lần thì tăng week_number
            // Điều này đảm bảo week_number là tuần tự trong phạm vi của tháng.
            int daysSinceMonthStart = currentDay.DayNumber - startOfMonth.DayNumber;
            currentWeekNumber = (daysSinceMonthStart / 7) + 1;


            var createWeekDto = new CreateWeekDto
            {
                ScheduleId = scheduleId,
                DayOfWeek = currentDay,
                WeekNumber = currentWeekNumber
            };

            await _weekService.AddAsync(createWeekDto); // Thêm week thông qua WeekService
            Console.WriteLine($"[ScheduleService] -- Created week: ScheduleId={scheduleId}, Day={currentDay}, WeekNumber={currentWeekNumber}");

            currentDay = currentDay.AddDays(1); // Di chuyển đến ngày tiếp theo
        }
        Console.WriteLine($"[ScheduleService] - Finished generating weeks for schedule ID {scheduleId}.");
    }
    
    // Phương thức mới để đảm bảo tất cả các schedule hiện có đều có weeks
    private async Task EnsureWeeksForExistingSchedulesAsync()
    {
        Console.WriteLine("[ScheduleService] Checking existing schedules for missing weeks...");

        // Lấy tất cả các schedule hiện có
        var allSchedules = await _unitOfWork.Schedules.GetAllAsync();

        foreach (var schedule in allSchedules)
        {
            if (!schedule.month_year.HasValue)
            {
                Console.WriteLine($"[ScheduleService] - Skipping schedule ID {schedule.schedule_id} because month_year is null.");
                continue;
            }

            // Kiểm tra xem schedule này đã có bất kỳ week nào chưa
            var existingWeeks = await _unitOfWork.Weeks.GetWeeksByScheduleIdAsync(schedule.schedule_id);

            if (!existingWeeks.Any())
            {
                // Nếu chưa có week nào, thì tạo week cho schedule này
                Console.WriteLine($"[ScheduleService] - Schedule ID {schedule.schedule_id} ({schedule.month_year.Value.Month}/{schedule.month_year.Value.Year}) has no weeks. Generating them now...");
                await GenerateWeeksForMonthAsync(schedule.schedule_id, schedule.month_year.Value);
            }
            else
            {
                // Console.WriteLine($"[ScheduleService] - Schedule ID {schedule.schedule_id} ({schedule.month_year.Value.Month}/{schedule.month_year.Value.Year}) already has {existingWeeks.Count()} weeks.");
            }
        }
        Console.WriteLine("[ScheduleService] Finished checking existing schedules for missing weeks.");
    }


    private ScheduleDto MapToScheduleDto(schedule model)
    {
        return new ScheduleDto
        {
            ScheduleId = model.schedule_id,
            MonthYear = model.month_year,
            Note = model.note,
            // KHÔNG có UserId ở đây, vì model không có public int? user_id
        };
    }
}