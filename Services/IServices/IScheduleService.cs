using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IScheduleService
{
    Task<IEnumerable<ScheduleDto>> GetAllAsync();
    Task<ScheduleDto?> GetByIDAsync(int id);
    Task<ScheduleDto> AddAsync(CreateScheduleDto createScheduleDto);
    Task UpdateAsync(UpdateScheduleDto updateScheduleDto);
    Task DeleteAsync(int id);

    Task<IEnumerable<ScheduleDto>> SearchByIdOrNoteAsync(int? id, string? note);
    Task<IEnumerable<ScheduleDto>> GetSchedulesInMonthYearAsync(int month, int year);

    // Đảm bảo tên phương thức đúng như triển khai
    Task EnsureFutureSchedulesInternalAsync(); // Phương thức tạo lịch trình tương lai
    Task CleanupOldSchedulesInternalAsync(); 
}