using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IScheduleRepository: IGenericRepository<schedule>
{
    Task<schedule?> GetByMonthYearExactAsync(DateOnly monthYear);

    // Lấy tất cả lịch biểu trong một tháng/năm cụ thể (dựa vào DateOnly)
    Task<IEnumerable<schedule>> GetSchedulesInMonthYearAsync(int month, int year);

    // Tìm kiếm lịch biểu theo ID hoặc ghi chú
    Task<IEnumerable<schedule>> SearchByIdOrNoteAsync(int? id, string? note);
    // Task<schedule> AddAsync(schedule entity);
    // Task UpdateAsync(schedule entity);
    // Task<bool> DeleteAsync(int scheduleId);
}