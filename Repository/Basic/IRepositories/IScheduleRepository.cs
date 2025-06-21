using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IScheduleRepository
{
    Task<List<schedule>> GetAllAsync();
    Task<schedule> GetByIDAsync(int id);
    Task<List<schedule>> SearchByIdOrNoteAsync(int? id = null, string? note = null);
    Task<List<schedule>> SearchByMonthYearAsync(int? month = null, int? year = null);
    Task<schedule> AddAsync(schedule entity);
    Task UpdateAsync(schedule entity);
    Task<bool> DeleteAsync(int scheduleId);
}