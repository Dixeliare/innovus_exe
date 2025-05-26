using Repository.Models;

namespace Services.IServices;

public interface IScheduleService
{
    Task<List<schedule>> GetAllAsync();

    Task<schedule> GetByIDAsync(int id);

    Task<List<schedule>> SearchByIdOrNoteAsync(int? id = null, string? note = null);

    Task<List<schedule>> SearchByMonthYearAsync(int? month = null, int? year = null);

    Task<int> CreateSchedule(schedule schedule);

    Task<int> UpdateSchedule(schedule schedule);

    Task<bool> DeleteAsync(int scheduleId);

    // Phương thức để xử lý logic tạo 6 tháng cho tương lai và dọn dẹp định kỳ schedule 3 tháng trước đó
    Task EnsureScheduleExistenceAndCleanupAsync();
}