using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IOpeningScheduleRepository: IGenericRepository<opening_schedule>
{
    Task<IEnumerable<opening_schedule>> GetAllAsync();
    Task<opening_schedule> GetByIdAsync(int id);
    Task<IEnumerable<opening_schedule>> GetAllWithDayOfWeeksAsync(); // THÊM: Method để load với day_of_weeks
    Task<IEnumerable<opening_schedule>> GetAllWithClassSessionsAsync(); // THÊM: Method để load với class sessions cho GetAll
    Task<opening_schedule?> GetByIdWithDayOfWeeksAsync(int id); // THÊM: Method để load với day_of_weeks
    Task<opening_schedule?> GetByIdWithClassSessionsAsync(int id); // THÊM: Method để load với class sessions
    Task<IEnumerable<opening_schedule>> SearchOpeningSchedulesAsync(
        string? classCode = null,
        DateOnly? openingDay = null,
        DateOnly? endDate = null,
        // ĐÃ XÓA: string? schedule = null,
        int? studentQuantity = null,
        bool? isAdvancedClass = null);
    
}