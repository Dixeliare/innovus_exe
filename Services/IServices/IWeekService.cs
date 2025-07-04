using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IWeekService
{
    Task<IEnumerable<week>> GetAllAsync();
    Task<week> GetByIdAsync(int id);
    Task<IEnumerable<WeekDto>> GetWeeksByScheduleIdAsync(int scheduleId);
    Task<WeekDto> AddAsync(CreateWeekDto createWeekDto);
    Task UpdateAsync(UpdateWeekDto updateWeekDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<week>> SearchWeeksAsync(DateOnly? dayOfWeek = null, int? scheduleId = null);
}