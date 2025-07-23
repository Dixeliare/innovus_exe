using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IDayRepository : IGenericRepository<day>
{
    Task<IEnumerable<day>> GetDaysByWeekIdAsync(int weekId);
    Task<IEnumerable<day>> GetDaysByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    // Có thể thêm eager loading nếu day có class_sessions
    Task<day?> GetDayWithClassSessionsAsync(int dayId);
    
    Task<IEnumerable<day>> SearchDaysAsync(DateOnly? dateOfDay = null, int? weekId = null, string? dayOfWeekName = null);
}