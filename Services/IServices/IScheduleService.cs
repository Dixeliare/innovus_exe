using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IScheduleService
{
    Task<IEnumerable<ScheduleDto>> GetAllAsync();

    Task<ScheduleDto> GetByIDAsync(int id);

    Task<IEnumerable<ScheduleDto>> SearchByIdOrNoteAsync(int? id, string? note);

    Task<IEnumerable<ScheduleDto>> SearchByMonthYearAsync(int? month, int? year);

    Task<ScheduleDto> AddAsync(CreateScheduleDto createScheduleDto);
    Task UpdateAsync(UpdateScheduleDto updateScheduleDto);

    Task DeleteAsync(int scheduleId);

    // Phương thức để xử lý logic tạo 6 tháng cho tương lai và dọn dẹp định kỳ schedule 3 tháng trước đó
    Task EnsureScheduleExistenceAndCleanupAsync();
}