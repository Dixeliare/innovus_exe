using DTOs;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class DayService : IDayService
{
    private readonly IUnitOfWork _unitOfWork;

    public DayService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DayDto>> GetAllDaysAsync()
    {
        var days = await _unitOfWork.Days.GetAllAsync();
        var dayDtos = new List<DayDto>();
        foreach (var day in days)
        {
            dayDtos.Add(MapDayToDayDto(day));
        }

        return dayDtos;
    }

    public async Task<DayDto?> GetDayByIdAsync(int id)
    {
        var day = await _unitOfWork.Days
            .GetDayWithClassSessionsAsync(id); // Sử dụng GetDayWithClassSessionsAsync để lấy thông tin chi tiết
        if (day == null)
        {
            throw new NotFoundException("Day", "ID", id);
        }

        return MapDayToDayDto(day);
    }

    public async Task<DayDto> CreateDayAsync(CreateDayDto createDayDto)
    {
        if (string.IsNullOrWhiteSpace(createDayDto.DayOfWeekName))
        {
            var errors = new Dictionary<string, string[]>
            {
                { nameof(createDayDto.DayOfWeekName), new[] { "Tên ngày trong tuần không được để trống." } }
            };
            throw new ValidationException(errors);
        }

        if (createDayDto.WeekId.HasValue)
        {
            var weekExists = await _unitOfWork.Weeks.AnyAsync(w => w.week_id == createDayDto.WeekId.Value);
            if (!weekExists)
            {
                throw new NotFoundException("Week", "ID", createDayDto.WeekId.Value);
            }
        }

        var dayToCreate = new day
        {
            week_id = createDayDto.WeekId,
            date_of_day = createDayDto.DateOfDay,
            day_of_week_name = createDayDto.DayOfWeekName,
            is_active = createDayDto.IsActive ?? true
        };

        var addedDay = await _unitOfWork.Days.AddAsync(dayToCreate);
        await _unitOfWork.CompleteAsync(); // Gọi CompleteAsync() ở đây để lưu thay đổi

        return MapDayToDayDto(addedDay);
    }

    public async Task<bool> UpdateDayAsync(UpdateDayDto updateDayDto)
    {
        var existingDay = await _unitOfWork.Days.GetByIdAsync(updateDayDto.DayId);
        if (existingDay == null)
        {
            throw new NotFoundException("Day", "ID", updateDayDto.DayId);
        }

        if (string.IsNullOrWhiteSpace(updateDayDto.DayOfWeekName))
        {
            var errors = new Dictionary<string, string[]>
            {
                {
                    nameof(updateDayDto.DayOfWeekName),
                    new[] { "Tên ngày trong tuần không được để trống khi cập nhật." }
                }
            };
            throw new ValidationException(errors);
        }

        if (updateDayDto.WeekId.HasValue && updateDayDto.WeekId != existingDay.week_id)
        {
            var weekExists = await _unitOfWork.Weeks.AnyAsync(w => w.week_id == updateDayDto.WeekId.Value);
            if (!weekExists)
            {
                throw new NotFoundException("Week", "ID", updateDayDto.WeekId.Value);
            }
        }

        existingDay.week_id = updateDayDto.WeekId;
        existingDay.date_of_day = updateDayDto.DateOfDay;
        existingDay.day_of_week_name = updateDayDto.DayOfWeekName;
        existingDay.is_active = updateDayDto.IsActive;

        await _unitOfWork.Days.UpdateAsync(existingDay);
        await _unitOfWork.CompleteAsync(); // Gọi CompleteAsync() ở đây để lưu thay đổi

        return true;
    }

    public async Task<bool> DeleteDayAsync(int id)
    {
        var dayExists = await _unitOfWork.Days.AnyAsync(d => d.day_id == id);
        if (!dayExists)
        {
            throw new NotFoundException("Day", "ID", id);
        }

        var hasClassSessions = await _unitOfWork.ClassSessions.AnyAsync(cs => cs.day_id == id);
        if (hasClassSessions)
        {
            throw new InvalidOperationException($"Không thể xóa Day có ID {id} vì nó còn chứa các Class Sessions.");
        }

        var result = await _unitOfWork.Days.DeleteAsync(id);
        await _unitOfWork.CompleteAsync(); // Gọi CompleteAsync() ở đây để lưu thay đổi
        return result;
    }

    public async Task<IEnumerable<DayDto>> SearchDaysAsync(DateOnly? dateOfDay, int? weekId, string? dayOfWeekName)
    {
        var days = await _unitOfWork.Days.SearchDaysAsync(dateOfDay, weekId, dayOfWeekName);
        var dayDtos = new List<DayDto>();
        foreach (var day in days)
        {
            dayDtos.Add(MapDayToDayDto(day));
        }

        return dayDtos;
    }

    // Phương thức ánh xạ thủ công từ entity 'day' sang 'DayDto'
    private DayDto MapDayToDayDto(day dayEntity)
    {
        if (dayEntity == null)
        {
            return null;
        }

        return new DayDto
        {
            DayId = dayEntity.day_id,
            WeekId = dayEntity.week_id,
            DateOfDay = dayEntity.date_of_day,
            DayOfWeekName = dayEntity.day_of_week_name,
            IsActive = dayEntity.is_active,
            Week = dayEntity.week != null
                ? new WeekDto
                {
                    WeekId = dayEntity.week.week_id,
                    StartDate = dayEntity.week.start_date,
                    EndDate = dayEntity.week.end_date,
                    NumActiveDays = dayEntity.week.num_active_days,
                    ScheduleId = dayEntity.week.schedule_id
                }
                : null,
            ClassSessions = dayEntity.class_sessions != null
                ? dayEntity.class_sessions
                    .Select(cs => new BaseClassSessionDto
                    {
                        ClassSessionId = cs.class_session_id,
                        ClassId = cs.class_id,
                        DayId = cs.day_id,
                        TimeSlotId = cs.time_slot_id,
                        RoomCode = cs.room_code
                    }).ToList()
                : new List<BaseClassSessionDto>()
        };
    }
}