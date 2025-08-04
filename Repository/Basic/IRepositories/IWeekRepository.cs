using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IWeekRepository : IGenericRepository<week>
{
    //Task<IEnumerable<week>> GetAllAsync();
    Task<week> GetByIdAsync(int id);
    //Task<IEnumerable<week>> GetWeeksByScheduleIdAsync(int scheduleId);
    //Task<IEnumerable<week>> SearchWeeksAsync(DateOnly? dayOfWeek = null, int? scheduleId = null);
    
    Task<IEnumerable<week>> GetAllWeeksWithDaysAsync();
    Task<week?> GetWeekByIdWithDaysAsync(int id);
    Task<IEnumerable<week>> GetWeeksByScheduleIdWithDaysAsync(int scheduleId);
    Task<IEnumerable<week>> GetWeeksByScheduleIdWithDaysAndClassSessionsAsync(int scheduleId);
    Task<week?> GetWeekByIdWithDaysAndClassSessionsAsync(int id);
    
    // Đã thay đổi tham số để phù hợp với Week model
    Task<IEnumerable<week>> SearchWeeksAsync(int? scheduleId = null, int? weekNumberInMonth = null, DateOnly? startDate = null, DateOnly? endDate = null);
    
    Task<IEnumerable<week>> GetAllWithDetailsAsync();
}