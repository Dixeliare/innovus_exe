using DTOs;

namespace Services.IServices;

public interface IDayService
{
    Task<IEnumerable<DayDto>> GetAllDaysAsync();
    Task<DayDto?> GetDayByIdAsync(int id);
    Task<DayDto> CreateDayAsync(CreateDayDto createDayDto);
    Task<bool> UpdateDayAsync(UpdateDayDto updateDayDto);
    Task<bool> DeleteDayAsync(int id);
    Task<IEnumerable<DayDto>> SearchDaysAsync(DateOnly? dateOfDay, int? weekId, string? dayOfWeekName);
}