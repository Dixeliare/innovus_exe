using Repository.Models;

namespace Services.IServices;

public interface IWeekService
{
    Task<List<week>> GetAll();
    Task<week> GetById(int id);
    Task<int> CreateAsync(week week);
    Task<int> UpdateAsync(week week);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<week>> SearchWeeksAsync(DateOnly? dayOfWeek = null, int? scheduleId = null);
}