using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IOpeningScheduleRepository: IGenericRepository<opening_schedule>
{
    Task<IEnumerable<opening_schedule>> GetAllAsync();
    Task<opening_schedule> GetByIdAsync(int id);
    // Task<opening_schedule> AddAsync(opening_schedule entity);
    // Task UpdateAsync(opening_schedule entity);
    // Task<bool> DeleteAsync(int id);

    Task<IEnumerable<opening_schedule>> SearchOpeningSchedulesAsync(
        string? classCode = null,
        DateOnly? openingDay = null,
        DateOnly? endDate = null,
        string? schedule = null,
        int? studentQuantity = null,
        bool? isAdvancedClass = null);
    
}