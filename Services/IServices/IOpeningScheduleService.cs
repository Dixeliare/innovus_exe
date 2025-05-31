using Repository.Models;

namespace Services.IServices;

public interface IOpeningScheduleService
{
    Task<IEnumerable<opening_schedule>> GetAllAsync();
    Task<opening_schedule> GetByIdAsync(int id);
    Task<int> CreateAsync(opening_schedule opening_schedule);
    Task<int> UpdateAsync(opening_schedule opening_schedule);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<opening_schedule>> SearchOpeningSchedulesAsync(
        string? subject = null,
        string? classCode = null,
        DateOnly? openingDay = null,
        DateOnly? endDate = null,
        string? schedule = null,
        int? studentQuantity = null,
        bool? isAdvancedClass = null);
}