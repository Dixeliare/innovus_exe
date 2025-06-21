using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface ITimeslotRepository: IGenericRepository<timeslot>
{
    Task<IEnumerable<timeslot>> GetAllAsync();
    Task<timeslot> GetByIdAsync(int id);
    // Task<timeslot> AddAsync(timeslot entity);
    // Task UpdateAsync(timeslot entity);
    // Task<bool> DeleteAsync(int timeslotId);
    Task<IEnumerable<timeslot>> SearchTimeslotsAsync(TimeOnly? startTime = null, TimeOnly? endTime = null);
    
}