using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IOpeningScheduleRepository: IGenericRepository<opening_schedule>
{
    Task<IEnumerable<opening_schedule>> GetAllAsync();
    Task<opening_schedule> GetByIdAsync(int id);
    Task<IEnumerable<opening_schedule>> SearchOpeningSchedulesAsync(
        string? classCode = null,
        DateOnly? openingDay = null,
        DateOnly? endDate = null,
        // ĐÃ XÓA: string? schedule = null,
        int? studentQuantity = null,
        bool? isAdvancedClass = null);
    
}