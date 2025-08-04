using DTOs;

namespace Services.IServices;

public interface IDayOfWeekLookupService
{
    Task<IEnumerable<DayOfWeekLookupDto>> GetAllDayOfWeekLookupsAsync();
    Task<DayOfWeekLookupDto> GetDayOfWeekLookupByIdAsync(int id);
}