using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class WeekService : IWeekService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WeekService> _logger; // Khai báo logger

    public WeekService(IUnitOfWork unitOfWork, ILogger<WeekService> logger) // Thêm logger vào constructor
    {
        _unitOfWork = unitOfWork;
        _logger = logger; // Gán logger
    }

    public async Task<IEnumerable<WeekDto>> GetAllAsync()
    {
        var weeks = await _unitOfWork.Weeks.GetAllWithDetailsAsync();
        return weeks.Select(MapToWeekDto);
    }

    public async Task<WeekDto> GetByIdAsync(int id)
    {
        var week = await _unitOfWork.Weeks.GetWeekByIdWithDaysAsync(id);
        if (week == null)
        {
            throw new NotFoundException("Week", "Id", id);
        }
        return MapToWeekDto(week);
    }

    public async Task<IEnumerable<WeekDto>> GetWeeksByScheduleIdAsync(int scheduleId)
    {
        var scheduleExists = await _unitOfWork.Schedules.GetByIdAsync(scheduleId);
        if (scheduleExists == null)
        {
            throw new NotFoundException("Schedule", "Id", scheduleId);
        }
        var weeks = await _unitOfWork.Weeks.GetWeeksByScheduleIdWithDaysAsync(scheduleId);
        return weeks.Select(MapToWeekDto);
    }

    public async Task<WeekDto> AddAsync(CreateWeekDto createWeekDto)
    {
        // Validation từ code của bạn
        if (createWeekDto.WeekNumberInMonth <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "WeekNumberInMonth", new string[] { "Số tuần trong tháng phải là một số dương hợp lệ." } }
            });
        }
        if (createWeekDto.StartDate == default(DateOnly))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "StartDate", new string[] { "Ngày bắt đầu không được để trống." } }
            });
        }
        if (createWeekDto.EndDate == default(DateOnly))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "EndDate", new string[] { "Ngày kết thúc không được để trống." } }
            });
        }
        if (createWeekDto.StartDate > createWeekDto.EndDate)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "DateRange", new string[] { "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc." } }
            });
        }

        var scheduleExists = await _unitOfWork.Schedules.GetByIdAsync(createWeekDto.ScheduleId);
        if (scheduleExists == null)
        {
            throw new NotFoundException("Schedule", "Id", createWeekDto.ScheduleId);
        }

        var existingWeeks = await _unitOfWork.Weeks.SearchWeeksAsync(createWeekDto.ScheduleId, createWeekDto.WeekNumberInMonth);

        if (existingWeeks.Any())
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "WeekNumberInMonth", new string[] { $"Tuần số {createWeekDto.WeekNumberInMonth} đã tồn tại cho lịch trình này." } }
            });
        }

        var weekEntity = new week
        {
            week_number_in_month = createWeekDto.WeekNumberInMonth,
            schedule_id = createWeekDto.ScheduleId,
            start_date = createWeekDto.StartDate,
            end_date = createWeekDto.EndDate,
            // num_active_days sẽ được tính toán sau khi tạo days
            num_active_days = 0 
        };

        try
        {
            // Bước 1: Thêm weekEntity vào context.
            // Điều này là quan trọng để addedWeek có thể được theo dõi bởi EF Core
            // và sau đó được sử dụng để thiết lập mối quan hệ với các ngày.
            var addedWeek = await _unitOfWork.Weeks.AddAsync(weekEntity); 

            // Bước 2: Tạo các ngày cho tuần đã thêm.
            // Truyền đối tượng 'addedWeek' đã được theo dõi
            var createdDays = GenerateDaysForWeekInternal(addedWeek, addedWeek.start_date, addedWeek.end_date).ToList();
        
            // Bước 3: Thêm tất cả các ngày đã tạo vào context.
            await _unitOfWork.Days.AddRangeAsync(createdDays); 

            // Bước 4: Cập nhật num_active_days của addedWeek.
            addedWeek.num_active_days = createdDays.Count(); 

            // Bước 5: Hoàn tất Unit of Work để lưu tất cả các thay đổi vào DB.
            await _unitOfWork.CompleteAsync(); 

            // Bước 6: Lấy lại tuần vừa thêm cùng với các ngày đã tạo (nếu cần cho DTO trả về).
            var addedWeekWithDays = await _unitOfWork.Weeks.GetWeekByIdWithDaysAsync(addedWeek.week_id);
            return MapToWeekDto(addedWeekWithDays);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("An error occurred while saving the week to the database.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("Đã xảy ra lỗi không mong muốn khi thêm tuần.", ex,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteWeeksByScheduleIdAsync(int scheduleId)
    {
        var weeksToDelete = await _unitOfWork.Weeks.GetWeeksByScheduleIdWithDaysAndClassSessionsAsync(scheduleId);

        if (weeksToDelete.Any())
        {
            foreach (var week in weeksToDelete)
            {
                if (week.days != null)
                {
                    foreach (var day in week.days)
                    {
                        if (day.class_sessions != null && day.class_sessions.Any())
                        {
                            throw new ApiException($"Không thể xóa tuần có ID {week.week_id} trong lịch trình này vì nó chứa các ngày có phiên học liên quan.", (int)HttpStatusCode.Conflict);
                        }
                    }
                }
            }
            
            // Xóa days và weeks bằng RemoveRange
            foreach (var week in weeksToDelete)
            {
                if (week.days != null && week.days.Any())
                {
                    _unitOfWork.Days.RemoveRange(week.days); // Sử dụng RemoveRange
                }
            }
            _unitOfWork.Weeks.RemoveRange(weeksToDelete); // Sử dụng RemoveRange
            // KHÔNG GỌI await _unitOfWork.CompleteAsync() Ở ĐÂY - ScheduleService sẽ gọi nó
        }
    }

    public async Task UpdateAsync(UpdateWeekDto updateWeekDto)
    {
        if (updateWeekDto.WeekId <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "WeekId", new string[] { "ID tuần không hợp lệ." } }
            });
        }

        var existingWeek = await _unitOfWork.Weeks.GetWeekByIdWithDaysAsync(updateWeekDto.WeekId);
        if (existingWeek == null)
        {
            throw new NotFoundException("Week", "Id", updateWeekDto.WeekId);
        }

        if (updateWeekDto.WeekNumberInMonth.HasValue)
        {
            if (existingWeek.week_number_in_month != updateWeekDto.WeekNumberInMonth.Value)
            {
                var existingWeeksWithSameNumber = await _unitOfWork.Weeks.SearchWeeksAsync(
                    updateWeekDto.ScheduleId ?? existingWeek.schedule_id,
                    updateWeekDto.WeekNumberInMonth.Value
                );
                if (existingWeeksWithSameNumber.Any(w => w.week_id != existingWeek.week_id))
                {
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        { "WeekNumberInMonth", new string[] { $"Tuần số {updateWeekDto.WeekNumberInMonth} đã tồn tại trong lịch trình này." } }
                    });
                }
            }
            existingWeek.week_number_in_month = updateWeekDto.WeekNumberInMonth.Value;
        }

        if (updateWeekDto.StartDate.HasValue)
        {
            existingWeek.start_date = updateWeekDto.StartDate.Value;
        }
        if (updateWeekDto.EndDate.HasValue)
        {
            existingWeek.end_date = updateWeekDto.EndDate.Value;
        }
        if (updateWeekDto.NumActiveDays.HasValue)
        {
            existingWeek.num_active_days = updateWeekDto.NumActiveDays.Value;
        }

        if (updateWeekDto.ScheduleId.HasValue && existingWeek.schedule_id != updateWeekDto.ScheduleId.Value)
        {
            var scheduleExists = await _unitOfWork.Schedules.GetByIdAsync(updateWeekDto.ScheduleId.Value);
            if (scheduleExists == null)
            {
                throw new NotFoundException("Schedule", "Id", updateWeekDto.ScheduleId.Value);
            }
            existingWeek.schedule_id = updateWeekDto.ScheduleId.Value;
        }

        try
        {
            await _unitOfWork.Weeks.UpdateAsync(existingWeek);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("An error occurred while updating the week in the database.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred during week update.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existingWeek = await _unitOfWork.Weeks.GetWeekByIdWithDaysAndClassSessionsAsync(id);
        if (existingWeek == null)
        {
            throw new NotFoundException("Week", "Id", id);
        }

        if (existingWeek.days != null)
        {
            foreach (var day in existingWeek.days)
            {
                if (day.class_sessions != null && day.class_sessions.Any())
                {
                    throw new ApiException($"Không thể xóa tuần có ID {id} vì nó có các ngày chứa phiên học liên quan.", (int)HttpStatusCode.Conflict);
                }
            }
        }

        try
        {
            if (existingWeek.days != null && existingWeek.days.Any())
            {
                _unitOfWork.Days.RemoveRange(existingWeek.days); // Sử dụng RemoveRange
            }
            var result = await _unitOfWork.Weeks.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            return result;
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("An error occurred while deleting the week from the database. It might have related records.", dbEx, (int)HttpStatusCode.Conflict);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred during week deletion.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<WeekDto>> SearchWeeksAsync(int? scheduleId = null, int? weekNumberInMonth = null)
    {
        var weeks = await _unitOfWork.Weeks.SearchWeeksAsync(scheduleId, weekNumberInMonth);
        return weeks.Select(MapToWeekDto);
    }

    public async Task<IEnumerable<WeekDto>> GenerateWeeksForMonthAsync(int scheduleId, int year, int month)
    {
        _logger.LogInformation($"[WeekService] - Starting to generate weeks and days for schedule ID {scheduleId}, {month}/{year}.");
        var generatedWeekDtos = new List<WeekDto>();

        var scheduleExists = await _unitOfWork.Schedules.GetByIdAsync(scheduleId);
        if (scheduleExists == null)
        {
            _logger.LogError($"[WeekService] - Schedule ID {scheduleId} not found when trying to generate weeks.");
            throw new NotFoundException("Schedule", "Id", scheduleId);
        }

        // Kỹ thuật này hợp lý khi bạn muốn tạo lại tuần, nhưng có thể xung đột với logic EnsureFutureSchedulesInternalAsync 
        // nếu nó được gọi lại cho một tháng đã có.
        // Bạn đã thay đổi logic EnsureFutureSchedulesInternalAsync trong ScheduleService để chỉ gọi AddAsync nếu không tồn tại, 
        // vậy nên việc kiểm tra này là an toàn.
        var existingWeeksWithDays = await _unitOfWork.Weeks.GetWeeksByScheduleIdWithDaysAsync(scheduleId);
        if (existingWeeksWithDays.Any())
        {
            _logger.LogWarning($"[WeekService] - Weeks and days already exist for Schedule ID {scheduleId}. Skipping generation.");
            throw new ApiException($"Các tuần và ngày đã tồn tại cho Lịch trình ID {scheduleId}. Vui lòng xóa chúng trước nếu bạn muốn tạo lại.", (int)HttpStatusCode.Conflict);
        }

        var firstDayOfMonth = new DateOnly(year, month, 1);
        var lastDayOfMonth = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        
        var currentWeekStartDate = firstDayOfMonth; 
        int weekCounter = 1;

        List<week> weeksToProcess = new List<week>();
        
        while (currentWeekStartDate <= lastDayOfMonth)
        {
            var weekEndDate = currentWeekStartDate.AddDays(6); 
            if (weekEndDate > lastDayOfMonth)
            {
                weekEndDate = lastDayOfMonth;
            }

            var newWeek = new week
            {
                schedule_id = scheduleId,
                week_number_in_month = weekCounter,
                start_date = currentWeekStartDate,
                end_date = weekEndDate,
                num_active_days = 0 
            };

            weeksToProcess.Add(newWeek);
            
            currentWeekStartDate = weekEndDate.AddDays(1);
            weekCounter++; 
        }
        
        // ======================================================================================
        // BƯỚC SỬA ĐỔI QUAN TRỌNG NHẤT: LƯU WEEKS TRƯỚC ĐỂ CÓ week_id THỰC TẾ
        // ======================================================================================
        if (weeksToProcess.Any())
        {
            await _unitOfWork.Weeks.AddRangeAsync(weeksToProcess);
            try
            {
                // LƯU CÁC WEEKS VÀO DATABASE NGAY LẬP TỨC.
                // Sau khi CompleteAsync(), các đối tượng 'week' trong 'weeksToProcess' 
                // sẽ có 'week_id' được gán từ database.
                await _unitOfWork.CompleteAsync(); 
                _logger.LogInformation($"[WeekService] - Successfully saved {weeksToProcess.Count} weeks for schedule ID {scheduleId}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[WeekService] - Error saving weeks for schedule ID {scheduleId}. Aborting day generation.");
                throw new ApiException("Có lỗi xảy ra khi lưu các tuần. Không thể tạo các ngày.", ex, (int)HttpStatusCode.InternalServerError);
            }
        }
        else
        {
            _logger.LogInformation($"[WeekService] - No weeks to generate for schedule ID {scheduleId}.");
            return generatedWeekDtos; // Trả về danh sách rỗng nếu không có tuần nào được tạo
        }

        // 2. Tạo và thêm Days cho mỗi Week sau khi week_id đã được gán
        List<day> allGeneratedDays = new List<day>();
        // Duyệt qua các week đã được lưu (có week_id)
        foreach (var week in weeksToProcess) 
        {
            // Bây giờ week.week_id đã có giá trị hợp lệ
            var daysToAdd = GenerateDaysForWeekInternal(week, week.start_date, week.end_date).ToList();
            allGeneratedDays.AddRange(daysToAdd);
            week.num_active_days = daysToAdd.Count(); // Cập nhật số ngày hoạt động
            _logger.LogInformation($"[WeekService] - Generated {daysToAdd.Count} days for week ID {week.week_id} (WeekNum: {week.week_number_in_month}).");
        }
        
        // Thêm tất cả days vào context
        await _unitOfWork.Days.AddRangeAsync(allGeneratedDays);

        // KHÔNG GỌI CompleteAsync() ở đây. ScheduleService sẽ gọi nó.
        _logger.LogInformation($"[WeekService] - Added all generated days to context for schedule ID {scheduleId}.");

        // Cập nhật lại các thuộc tính của Week nếu có thay đổi (ví dụ: num_active_days)
        // Các week trong weeksToProcess đã được theo dõi và các thay đổi sẽ được lưu khi ScheduleService gọi CompleteAsync()
        
        // Trả về DTO sau khi các thực thể đã được thêm vào context
        // Để làm điều này, chúng ta cần lấy lại các Week với Days đã được tải
        var finalWeeks = await _unitOfWork.Weeks.GetWeeksByScheduleIdWithDaysAsync(scheduleId);
        _logger.LogInformation($"[WeekService] - Finished generating weeks and days for schedule ID {scheduleId}.");
        return finalWeeks.Select(MapToWeekDto);
    }


    // ====================================================================================
    // HÀM MỚI: TẠO CÁC NGÀY CHO MỘT TUẦN CỤ THỂ (INTERNAL)
    // Đã điều chỉnh để nhận đối tượng `week` cha để thiết lập mối quan hệ
    // ====================================================================================
    private IEnumerable<day> GenerateDaysForWeekInternal(week parentWeek, DateOnly startDate, DateOnly endDate)
    {
        var generatedDays = new List<day>();
        DateOnly currentDay = startDate;

        while (currentDay <= endDate)
        {
            var newDay = new day
            {
                week = parentWeek, // Gán trực tiếp đối tượng tuần cha
                date_of_day = currentDay,
                day_of_week_name = currentDay.DayOfWeek.ToString(),
                is_active = true 
            };
            generatedDays.Add(newDay);
            currentDay = currentDay.AddDays(1);
        }
        return generatedDays;
    }

    // ====================================================================================
    // HÀM MAP DTO: Giữ nguyên logic map days như trong code gốc của bạn
    // ====================================================================================
    private WeekDto MapToWeekDto(week model)
    {
        return new WeekDto
        {
            WeekId = model.week_id,
            WeekNumberInMonth = model.week_number_in_month,
            ScheduleId = model.schedule_id,
            StartDate = model.start_date,
            EndDate = model.end_date,
            NumActiveDays = model.num_active_days,
            Schedule = model.schedule != null ? new ScheduleDto
            {
                ScheduleId = model.schedule.schedule_id,
                MonthYear = model.schedule.month_year,
                Note = model.schedule.note
            } : null,
            Days = model.days?.Select(d => new DayDto
            {
                DayId = d.day_id,
                WeekId = d.week_id,
                DateOfDay = d.date_of_day,
                DayOfWeekName = d.day_of_week_name,
                IsActive = d.is_active,
                Week = d.week != null ? new WeekDto
                {
                    WeekId = d.week.week_id,
                    WeekNumberInMonth = d.week.week_number_in_month,
                    ScheduleId = d.week.schedule_id,
                    StartDate = d.week.start_date,
                    EndDate = d.week.end_date,
                    NumActiveDays = d.week.num_active_days
                } : null,
                ClassSessions = d.class_sessions?.Select(cs => new BaseClassSessionDto
                {
                    ClassSessionId = cs.class_session_id,
                    SessionNumber = cs.session_number,
                    Date = cs.date,
                    ClassId = cs.class_id,
                    DayId = cs.day_id,
                    TimeSlotId = cs.time_slot_id,
                    RoomCode = cs.room?.room_code
                }).ToList() ?? new List<BaseClassSessionDto>()
            }).ToList() ?? new List<DayDto>()
        };
    }
}