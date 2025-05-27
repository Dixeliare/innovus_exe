using Repository.Models;

namespace Services.IServices;

public interface ITimeslotService
{
    Task<IEnumerable<timeslot>> GetAllAsync();

    Task<timeslot> GetByIDAsync(int id);

    Task<IEnumerable<timeslot>> SearchByStartTimeOrEndTimeAsync(TimeOnly? startTime = null, TimeOnly? endTime = null);

    Task<int> CreateTimeslot(timeslot timeslot);

    Task<int> UpdateTimeSlot(timeslot timeslot);

    Task<bool> DeleteAsync(int timeslotId);
}