using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IWeekRepository : IGenericRepository<week>
{
    Task<IEnumerable<week>> GetAllAsync();
    Task<week> GetByIdAsync(int id);
    Task<IEnumerable<week>> GetWeeksByScheduleIdAsync(int scheduleId);
    Task<IEnumerable<week>> SearchWeeksAsync(DateOnly? dayOfWeek = null, int? scheduleId = null);
    
}