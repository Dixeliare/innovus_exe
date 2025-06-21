using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IWeekRepository
{
    Task<IEnumerable<week>> GetAll();
    Task<week> GetById(int id);
    Task<week> AddAsync(week entity);
    Task UpdateAsync(week entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<week>> GetWeeksByScheduleIdAsync(int scheduleId);
    Task<IEnumerable<week>> SearchWeeksAsync(DateOnly? dayOfWeek = null, int? scheduleId = null);
    
}