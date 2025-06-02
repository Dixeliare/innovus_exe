using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IOpeningScheduleService
{
    Task<IEnumerable<opening_schedule>> GetAllAsync();
    Task<opening_schedule> GetByIdAsync(int id);
    Task<OpeningScheduleDto> AddAsync(CreateOpeningScheduleDto createOpeningScheduleDto);
    Task UpdateAsync(UpdateOpeningScheduleDto updateOpeningScheduleDto);
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