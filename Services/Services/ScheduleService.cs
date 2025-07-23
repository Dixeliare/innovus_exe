using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWeekService _weekService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ScheduleService> _logger; // Đã thêm Logger

    public ScheduleService(IUnitOfWork unitOfWork, IWeekService weekService, IConfiguration configuration, ILogger<ScheduleService> logger)
    {
        _unitOfWork = unitOfWork;
        _weekService = weekService;
        _configuration = configuration;
        _logger = logger; // Khởi tạo Logger
    }

    public async Task<IEnumerable<ScheduleDto>> GetAllAsync()
    {
        var schedules = await _unitOfWork.Schedules.GetAllAsync();
        return schedules.Select(s => MapToScheduleDto(s));
    }

    public async Task<ScheduleDto?> GetByIDAsync(int id)
    {
        var schedule = await _unitOfWork.Schedules.GetByIdAsync(id);
        if (schedule == null)
        {
            throw new NotFoundException("Schedule", "Id", id);
        }

        return MapToScheduleDto(schedule);
    }

    public async Task<ScheduleDto> AddAsync(CreateScheduleDto createScheduleDto)
    {
        if (!createScheduleDto.MonthYear.HasValue)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "MonthYear", new[] { "MonthYear is required." } }
            });
        }

        var existingSchedule = await _unitOfWork.Schedules.GetByMonthYearExactAsync(createScheduleDto.MonthYear.Value);
        if (existingSchedule != null)
        {
            throw new ApiException($"Lịch trình cho ngày {createScheduleDto.MonthYear.Value.ToShortDateString()} đã tồn tại.",
                (int)HttpStatusCode.Conflict);
        }

        var schedule = new schedule
        {
            month_year = createScheduleDto.MonthYear,
            note = createScheduleDto.Note
        };

        try
        {
            var addedSchedule = await _unitOfWork.Schedules.AddAsync(schedule); 
            
            // LƯU LỊCH TRÌNH NGAY LẬP TỨC để lấy schedule_id được sinh ra bởi database.
            // Điều này là cần thiết để các Weeks và Days liên quan có thể tham chiếu đúng schedule_id.
            await _unitOfWork.CompleteAsync(); 

            if (addedSchedule.month_year.HasValue)
            {
                // Gọi GenerateWeeksForMonthAsync từ WeekService
                // Sử dụng schedule_id ĐÃ ĐƯỢC CẬP NHẬT từ database.
                await _weekService.GenerateWeeksForMonthAsync(
                    addedSchedule.schedule_id, 
                    addedSchedule.month_year.Value.Year,
                    addedSchedule.month_year.Value.Month
                );
            }
            else
            {
                throw new ApiException("Không thể tạo tuần vì thông tin tháng/năm không hợp lệ trong lịch trình đã tạo.", (int)HttpStatusCode.InternalServerError);
            }

            // LƯU LẠI LẦN THỨ HAI để lưu Weeks và Days vừa được thêm vào context bởi WeekService.
            await _unitOfWork.CompleteAsync(); 

            return MapToScheduleDto(addedSchedule);
        }
        catch (DbUpdateException dbEx)
        {
            // Log chi tiết hơn để dễ debug
            _logger.LogError(dbEx, "DbUpdateException during Schedule AddAsync.");
            if (dbEx.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true ||
                (dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505"))
            {
                throw new ApiException($"Lịch trình cho ngày {createScheduleDto.MonthYear.Value.ToShortDateString()} đã tồn tại.",
                                       (int)HttpStatusCode.Conflict);
            }

            throw new ApiException("Có lỗi xảy ra khi tạo lịch trình.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Schedule AddAsync.");
            throw new ApiException("Đã xảy ra lỗi không mong muốn khi tạo lịch trình.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task UpdateAsync(UpdateScheduleDto updateScheduleDto)
    {
        var existingSchedule = await _unitOfWork.Schedules.GetByIdAsync(updateScheduleDto.ScheduleId);
        if (existingSchedule == null)
        {
            throw new NotFoundException("Schedule", "Id", updateScheduleDto.ScheduleId);
        }

        if (!updateScheduleDto.MonthYear.HasValue)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "MonthYear", new[] { "MonthYear is required." } }
            });
        }

        if (existingSchedule.month_year != updateScheduleDto.MonthYear.Value)
        {
            var scheduleWithSameMonthYear =
                await _unitOfWork.Schedules.GetByMonthYearExactAsync(updateScheduleDto.MonthYear.Value);
            if (scheduleWithSameMonthYear != null &&
                scheduleWithSameMonthYear.schedule_id != updateScheduleDto.ScheduleId)
            {
                throw new ApiException($"Lịch trình cho ngày {updateScheduleDto.MonthYear.Value.ToShortDateString()} đã tồn tại.",
                    (int)HttpStatusCode.Conflict);
            }
        }

        existingSchedule.month_year = updateScheduleDto.MonthYear;
        existingSchedule.note = updateScheduleDto.Note;

        try
        {
            await _unitOfWork.Schedules.UpdateAsync(existingSchedule);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DbUpdateException during Schedule UpdateAsync.");
            if (dbEx.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true ||
                (dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505"))
            {
                throw new ApiException($"Lịch trình cho ngày {updateScheduleDto.MonthYear.Value.ToShortDateString()} đã tồn tại.",
                                       (int)HttpStatusCode.Conflict);
            }

            throw new ApiException("Có lỗi xảy ra khi cập nhật lịch trình.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during schedule update.");
            throw new ApiException("An unexpected error occurred during schedule update.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var scheduleToDelete = await _unitOfWork.Schedules.GetByIdAsync(id);
        if (scheduleToDelete == null)
        {
            throw new NotFoundException("Schedule", "Id", id);
        }

        try
        {
            // Xóa các tuần và ngày liên quan trước
            // Phương thức này chỉ đánh dấu các entities để xóa, không tự CompleteAsync()
            await _weekService.DeleteWeeksByScheduleIdAsync(id); 
            
            // Xóa lịch trình chính
            await _unitOfWork.Schedules.DeleteAsync(id);
            
            await _unitOfWork.CompleteAsync(); // Lưu tất cả các thay đổi (xóa weeks, days, schedule)
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DbUpdateException during Schedule DeleteAsync.");
            throw new ApiException("Không thể xóa lịch trình do có các bản ghi liên quan (ràng buộc khóa ngoại).", dbEx,
                (int)HttpStatusCode.Conflict);
        }
        catch (ApiException)
        {
            throw; // Re-throw ApiException from DeleteWeeksByScheduleIdAsync
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during schedule deletion.");
            throw new ApiException("An unexpected error occurred during schedule deletion.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<ScheduleDto>> SearchByIdOrNoteAsync(int? id, string? note)
    {
        var schedules = await _unitOfWork.Schedules.SearchByIdOrNoteAsync(id, note);
        return schedules.Select(s => MapToScheduleDto(s));
    }

    public async Task<IEnumerable<ScheduleDto>> GetSchedulesInMonthYearAsync(int month, int year)
    {
        var schedules = await _unitOfWork.Schedules.GetSchedulesInMonthYearAsync(month, year);
        return schedules.Select(s => MapToScheduleDto(s));
    }

    public async Task EnsureFutureSchedulesInternalAsync()
    {
        var currentDateTime = DateTime.UtcNow;
        var schedulesToCreate = new List<schedule>(); 

        int numberOfMonthsToGenerate = _configuration.GetValue<int>("ScheduleSettings:MonthsToGenerate", 3);

        for (int i = 0; i < numberOfMonthsToGenerate; i++)
        {
            var targetDate = currentDateTime.AddMonths(i);
            var targetMonthYear = new DateOnly(targetDate.Year, targetDate.Month, 1);

            var existingSchedule = await _unitOfWork.Schedules.GetByMonthYearExactAsync(targetMonthYear);
            if (existingSchedule == null)
            {
                schedulesToCreate.Add(new schedule
                {
                    month_year = targetMonthYear,
                    note = $"Tự động tạo cho tháng {targetMonthYear.Month}/{targetMonthYear.Year}"
                });
            }
        }

        if (schedulesToCreate.Any())
        {
            _logger.LogInformation($"Found {schedulesToCreate.Count} new schedules to generate.");
            
            // Thêm tất cả schedule vào context.
            await _unitOfWork.Schedules.AddRangeAsync(schedulesToCreate); 
            
            // LƯU CÁC SCHEDULE MỚI để có ID thực tế trước khi tạo Weeks/Days
            try
            {
                 await _unitOfWork.CompleteAsync(); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving new schedules in EnsureFutureSchedulesInternalAsync. Skipping week generation for these schedules.");
                // Nếu không lưu được schedule, không tạo weeks cho nó nữa.
                return; 
            }
           

            foreach (var newSchedule in schedulesToCreate)
            {
                if (newSchedule.month_year.HasValue)
                {
                    _logger.LogInformation($"Generating weeks and days for Schedule ID: {newSchedule.schedule_id} (Month: {newSchedule.month_year.Value.Month}/{newSchedule.month_year.Value.Year})");
                    await _weekService.GenerateWeeksForMonthAsync(
                        newSchedule.schedule_id,
                        newSchedule.month_year.Value.Year,
                        newSchedule.month_year.Value.Month
                    );
                }
            }
            // LƯU TẤT CẢ CÁC WEEKS VÀ DAYS ĐƯỢC TẠO SAU KHI ĐÃ CÓ ID CỦA SCHEDULE
            await _unitOfWork.CompleteAsync(); 
            _logger.LogInformation("Successfully generated weeks and days for new schedules.");
        }
        else
        {
            _logger.LogInformation("No future schedules needed to be generated.");
        }
    }

    public async Task CleanupOldSchedulesInternalAsync()
    {
        var currentDateTime = DateTime.UtcNow;
        int monthsToKeepPast = _configuration.GetValue<int>("ScheduleSettings:MonthsToKeepPast", 1);

        var thresholdDate = currentDateTime.AddMonths(-monthsToKeepPast);
        var thresholdMonthYear = new DateOnly(thresholdDate.Year, thresholdDate.Month, 1);

        // Lấy tất cả và lọc ngay lập tức sau khi await
        var oldSchedules = (await _unitOfWork.Schedules.GetAllAsync()) 
            .Where(s => s.month_year.HasValue && s.month_year.Value < thresholdMonthYear)
            .ToList(); 

        if (oldSchedules.Any())
        {
            _logger.LogInformation($"Found {oldSchedules.Count} old schedules to clean up.");
            foreach (var s in oldSchedules)
            {
                try
                {
                    await _weekService.DeleteWeeksByScheduleIdAsync(s.schedule_id);
                    _unitOfWork.Schedules.Remove(s);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, $"Error deleting old schedule {s.schedule_id} due to related entities. Skipping this schedule.");
                }
                catch (ApiException apiEx)
                {
                    _logger.LogError(apiEx, $"Error deleting old schedule {s.schedule_id}. Skipping this schedule.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while processing old schedule {s.schedule_id} for deletion. Skipping this schedule.");
                }
            }
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Cleanup of old schedules completed.");
        }
        else
        {
            _logger.LogInformation("No old schedules needed to be cleaned up.");
        }
    }

    private ScheduleDto MapToScheduleDto(schedule model)
    {
        return new ScheduleDto
        {
            ScheduleId = model.schedule_id,
            MonthYear = model.month_year,
            Note = model.note
        };
    }
}