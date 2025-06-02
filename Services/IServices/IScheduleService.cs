using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IScheduleService
{
    Task<List<schedule>> GetAllAsync();

    Task<schedule> GetByIDAsync(int id);

    Task<List<schedule>> SearchByIdOrNoteAsync(int? id, string? note);

    Task<List<schedule>> SearchByMonthYearAsync(int? month, int? year);

    Task<ScheduleDto> AddAsync(CreateScheduleDto createScheduleDto);
    Task UpdateAsync(UpdateScheduleDto updateScheduleDto);

    Task<bool> DeleteAsync(int scheduleId);

    // Phương thức để xử lý logic tạo 6 tháng cho tương lai và dọn dẹp định kỳ schedule 3 tháng trước đó
    Task EnsureScheduleExistenceAndCleanupAsync();
}