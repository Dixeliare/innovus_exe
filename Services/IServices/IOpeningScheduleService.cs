using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IOpeningScheduleService
{
    Task<IEnumerable<OpeningScheduleDto>> GetAllAsync();
    Task<OpeningScheduleDto> GetByIdAsync(int id);
    Task<OpeningScheduleDto> AddAsync(CreateOpeningScheduleDto createOpeningScheduleDto);
    Task UpdateAsync(UpdateOpeningScheduleDto updateOpeningScheduleDto);
    Task DeleteAsync(int id);

    Task<IEnumerable<OpeningScheduleDto>> SearchOpeningSchedulesAsync(
        string? classCode = null,
        DateOnly? openingDay = null,
        DateOnly? endDate = null,
        string? schedule = null,
        int? studentQuantity = null,
        bool? isAdvancedClass = null);
}