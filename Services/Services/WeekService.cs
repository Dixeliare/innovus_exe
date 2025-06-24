using System.Net;
using DTOs;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class WeekService : IWeekService
{
    // private readonly IWeekRepository _weekRepository;
    // private readonly IScheduleRepository _scheduleRepository; // Để kiểm tra khóa ngoại

    private readonly IUnitOfWork _unitOfWork;

    // public WeekService(IWeekRepository weekRepository, IScheduleRepository scheduleRepository)
    // {
    //     _weekRepository = weekRepository;
    //     _scheduleRepository = scheduleRepository;
    // }

    public WeekService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<week>> GetAllAsync()
    {
        return await _unitOfWork.Weeks.GetAllAsync();
    }

    public async Task<week?> GetByIdAsync(int id)
    {
        var week = await _unitOfWork.Weeks.GetByIdAsync(id);
        if (week == null)
        {
            // NÉM NotFoundException khi không tìm thấy Week
            throw new NotFoundException("Week", "Id", id);
        }

        return week;
    }

    public async Task<IEnumerable<WeekDto>> GetWeeksByScheduleIdAsync(int scheduleId)
    {
        // Bạn có thể kiểm tra xem ScheduleId có tồn tại không ở đây
        var scheduleExists = await _unitOfWork.Schedules.GetByIdAsync(scheduleId); // Sửa GetByIDAsync -> GetByIdAsync
        if (scheduleExists == null)
        {
            throw new NotFoundException("Schedule", "Id", scheduleId);
        }

        var weeks = await _unitOfWork.Weeks.GetWeeksByScheduleIdAsync(scheduleId);
        // Có thể trả về danh sách rỗng nếu không có tuần nào, không nhất thiết phải ném lỗi 404
        // nếu danh sách rỗng là một kết quả hợp lệ.
        return weeks.Select(MapToWeekDto);
    }

    // CREATE Week
    public async Task<WeekDto> AddAsync(CreateWeekDto createWeekDto)
    {
        // 1. Validation dữ liệu đầu vào đơn giản (nếu chưa có Data Annotations)
        if (!createWeekDto.WeekNumber.HasValue || createWeekDto.WeekNumber.Value <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "WeekNumber", new string[] { "Số tuần phải là một số dương hợp lệ." } }
            });
        }

        if (!createWeekDto.DayOfWeek.HasValue)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "DayOfWeek", new string[] { "Ngày trong tuần không được để trống." } }
            });
        }
        // Thêm các validation khác nếu cần

        // 2. Kiểm tra khóa ngoại Schedule
        // Sửa GetByIDAsync -> GetByIdAsync
        var scheduleExists = await _unitOfWork.Schedules.GetByIdAsync(createWeekDto.ScheduleId);
        if (scheduleExists == null)
        {
            // NÉM NotFoundException khi ScheduleId không tồn tại
            throw new NotFoundException("Schedule", "Id", createWeekDto.ScheduleId);
        }

        // 3. Kiểm tra tính duy nhất hoặc các ràng buộc nghiệp vụ khác
        // Ví dụ: Không cho phép tạo tuần trùng số tuần và lịch trình
        var existingWeek = await _unitOfWork.Weeks.SearchWeeksAsync(
            createWeekDto.DayOfWeek, createWeekDto.ScheduleId);

        if (existingWeek.Any(w =>
                w.week_number == createWeekDto.WeekNumber && w.schedule_id == createWeekDto.ScheduleId))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "WeekNumber", new string[] { $"Tuần số {createWeekDto.WeekNumber} đã tồn tại cho lịch trình này." } }
            });
        }


        var weekEntity = new week
        {
            week_number = createWeekDto.WeekNumber,
            day_of_week = createWeekDto.DayOfWeek,
            schedule_id = createWeekDto.ScheduleId
        };

        try
        {
            var addedWeek = await _unitOfWork.Weeks.AddAsync(weekEntity); // Gọi AddAsync của GenericRepository
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
            return MapToWeekDto(addedWeek);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            // Bắt lỗi DB cụ thể nếu muốn chuyển đổi thành ngoại lệ nghiệp vụ
            // Ví dụ: Nếu có UNIQUE constraint violation trên DB (mà bạn chưa validate trước đó)
            if (existingWeek.Any(w =>
                    w.week_number == createWeekDto.WeekNumber && w.schedule_id == createWeekDto.ScheduleId))
            {
                throw new ValidationException(new Dictionary<string, string[]> // <-- SỬA TẠI ĐÂY: Thêm [] vào string
                {
                    {
                        "WeekNumber",
                        new string[] { $"Tuần số {createWeekDto.WeekNumber} đã tồn tại cho lịch trình này." }
                    } // <-- SỬA TẠI ĐÂY: Bọc chuỗi lỗi trong new string[]{}
                });
            }

            // Các lỗi DB khác, ném ApiException
            throw new ApiException("An error occurred while saving the week to the database.", dbEx,
                (int)HttpStatusCode.InternalServerError);
        }
    }

    // UPDATE Week
    public async Task UpdateAsync(UpdateWeekDto updateWeekDto)
    {
        // 1. Validation ID
        if (updateWeekDto.WeekId <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "WeekId", new string[] { "ID tuần không hợp lệ." } }
            });
        }

        // 2. Tìm Week hiện có. NÉM NotFoundException nếu không tìm thấy
        var existingWeek = await _unitOfWork.Weeks.GetByIdAsync(updateWeekDto.WeekId);
        if (existingWeek == null)
        {
            throw new NotFoundException("Week", "Id", updateWeekDto.WeekId);
        }

        // 3. Cập nhật các trường nếu có giá trị được cung cấp
        if (updateWeekDto.WeekNumber.HasValue)
        {
            existingWeek.week_number = updateWeekDto.WeekNumber.Value;
        }
        if (updateWeekDto.DayOfWeek.HasValue)
        {
            existingWeek.day_of_week = updateWeekDto.DayOfWeek.Value;
        }

        // 4. Cập nhật ScheduleId nếu được cung cấp và khác với giá trị hiện tại
        if (updateWeekDto.ScheduleId.HasValue && existingWeek.schedule_id != updateWeekDto.ScheduleId.Value)
        {
            // Sửa GetByIDAsync -> GetByIdAsync
            var scheduleExists = await _unitOfWork.Schedules.GetByIdAsync(updateWeekDto.ScheduleId.Value); 
            if (scheduleExists == null)
            {
                // NÉM NotFoundException khi ScheduleId không tồn tại cho việc cập nhật
                throw new NotFoundException("Schedule", "Id", updateWeekDto.ScheduleId.Value);
            }
            existingWeek.schedule_id = updateWeekDto.ScheduleId.Value;
        }

        try
        {
            await _unitOfWork.Weeks.UpdateAsync(existingWeek); // Gọi UpdateAsync của GenericRepository
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            // Xử lý lỗi DB tương tự như AddAsync
            throw new ApiException("An error occurred while updating the week in the database.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // 1. Tìm Week để xóa. NÉM NotFoundException nếu không tìm thấy
        var existingWeek = await _unitOfWork.Weeks.GetByIdAsync(id);
        if (existingWeek == null)
        {
            throw new NotFoundException("Week", "Id", id);
        }

        // 2. (Optional) Thêm các ràng buộc nghiệp vụ trước khi xóa
        // Ví dụ: Không thể xóa tuần nếu nó đã có class_sessions liên quan
        if (existingWeek.class_sessions != null && existingWeek.class_sessions.Any())
        {
            throw new ApiException($"Không thể xóa tuần có ID {id} vì nó có các phiên học liên quan.", (int)HttpStatusCode.Conflict); // HTTP 409 Conflict
        }
        
        try
        {
            var result = await _unitOfWork.Weeks.DeleteAsync(id); // Gọi DeleteAsync của GenericRepository
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
            return result;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            // Nếu có lỗi ràng buộc khóa ngoại từ DB, ném ApiException
            throw new ApiException("An error occurred while deleting the week from the database. It might have related records.", dbEx, (int)HttpStatusCode.Conflict); // HTTP 409 Conflict
        }
    }

    public async Task<IEnumerable<week>> SearchWeeksAsync(DateOnly? dayOfWeek, int? scheduleId)
    {
        return await _unitOfWork.Weeks.SearchWeeksAsync(dayOfWeek, scheduleId);
    }

    private WeekDto MapToWeekDto(week model)
    {
        return new WeekDto
        {
            WeekId = model.week_id,
            WeekNumber = model.week_number,
            DayOfWeek = model.day_of_week,
            ScheduleId = model.schedule_id
        };
    }
}