using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IWeekService
{
    Task<IEnumerable<WeekDto>> GetAllAsync(); // Trả về DTOs
    Task<WeekDto> GetByIdAsync(int id); // Trả về DTO
    Task<IEnumerable<WeekDto>> GetWeeksByScheduleIdAsync(int scheduleId);
    Task DeleteWeeksByScheduleIdAsync(int scheduleId);
    Task<WeekDto> AddAsync(CreateWeekDto createWeekDto);
    Task UpdateAsync(UpdateWeekDto updateWeekDto);
    Task<bool> DeleteAsync(int id);
    // XÓA DayOfWeek khỏi tham số tìm kiếm, chỉ tìm kiếm theo ScheduleId
    Task<IEnumerable<WeekDto>> SearchWeeksAsync(int? scheduleId = null, int? weekNumberInMonth = null); 
    Task<IEnumerable<WeekDto>> GenerateWeeksForMonthAsync(int scheduleId, int year, int month);
}