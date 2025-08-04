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
        // Sử dụng GetAllWithClassSessionsAsync để lấy tất cả days với include navigation properties
        var days = await _unitOfWork.Days.GetAllWithClassSessionsAsync();
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

        // Nếu week_id là bắt buộc trong model 'day', thì bạn phải đảm bảo nó có giá trị ở đây.
        // Nếu createDayDto.WeekId không có giá trị (null), bạn sẽ phải quyết định xử lý thế nào:
        // 1. Coi đây là lỗi và yêu cầu WeekId.
        // 2. Gán một giá trị mặc định hợp lệ (nhưng thường không nên với khóa ngoại).
        // 3. Hoặc cho phép tạo Day mà không có WeekId, nhưng điều này đi ngược lại việc bạn đã đổi week_id thành non-nullable.

        // Giả định rằng mỗi Day luôn phải có một WeekId hợp lệ:
        if (!createDayDto.WeekId.HasValue)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(createDayDto.WeekId), new[] { "WeekId là bắt buộc khi tạo ngày." } }
            });
        }

        var weekExists = await _unitOfWork.Weeks.AnyAsync(w => w.week_id == createDayDto.WeekId.Value);
        if (!weekExists)
        {
            throw new NotFoundException("Week", "ID", createDayDto.WeekId.Value);
        }

        var dayToCreate = new day
        {
            // Gán .Value vì chúng ta đã kiểm tra .HasValue ở trên
            week_id = createDayDto.WeekId.Value, 
            date_of_day = createDayDto.DateOfDay,
            day_of_week_name = createDayDto.DayOfWeekName,
            is_active = createDayDto.IsActive ?? true
        };

        var addedDay = await _unitOfWork.Days.AddAsync(dayToCreate);
        await _unitOfWork.CompleteAsync();

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

    existingDay.date_of_day = updateDayDto.DateOfDay;
    existingDay.day_of_week_name = updateDayDto.DayOfWeekName;

    // WeekId is nullable in UpdateDayDto, but non-nullable in entity.
    // Nếu updateDayDto.WeekId có giá trị, chúng ta cập nhật.
    // Nếu nó là null, chúng ta KHÔNG làm gì với existingDay.week_id
    // (giữ nguyên giá trị hiện có), vì bạn không thể gán null cho một int.
    if (updateDayDto.WeekId.HasValue) 
    {
        if (updateDayDto.WeekId.Value != existingDay.week_id) // Chỉ cập nhật nếu khác
        {
            var weekExists = await _unitOfWork.Weeks.AnyAsync(w => w.week_id == updateDayDto.WeekId.Value);
            if (!weekExists)
            {
                throw new NotFoundException("Week", "ID", updateDayDto.WeekId.Value);
            }
            existingDay.week_id = updateDayDto.WeekId.Value;
        }
    }
    // else { existingDay.week_id = null; } <-- XÓA DÒNG NÀY VÀ KHỐI NÀY
    // Lý do: existingDay.week_id giờ là `int`, không thể gán `null`.
    // Nếu bạn muốn cho phép "xóa" mối quan hệ tuần, bạn sẽ phải xem xét lại thiết kế model của mình
    // hoặc có một ID week đặc biệt cho trường hợp "không có tuần".

    if (updateDayDto.IsActive.HasValue)
    {
        existingDay.is_active = updateDayDto.IsActive.Value;
    }
    else 
    {
        // Nếu IsActive được set là null trong DTO, có thể bạn muốn giữ nguyên giá trị hiện tại
        // hoặc gán một giá trị mặc định nếu is_active trong model cũng đổi thành non-nullable.
        // Hiện tại existingDay.is_active vẫn là bool? nên gán null là OK.
        existingDay.is_active = null; 
    }
    
    await _unitOfWork.Days.UpdateAsync(existingDay);
    await _unitOfWork.CompleteAsync();

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
                        SessionNumber = cs.session_number,
                        Date = cs.date,
                        ClassId = cs.class_id,
                        DayId = cs.day_id,
                        TimeSlotId = cs.time_slot_id,
                        RoomCode = cs.room?.room_code // Lấy RoomCode từ navigation property 'room'
                    }).ToList()
                : new List<BaseClassSessionDto>()
        };
    }
}