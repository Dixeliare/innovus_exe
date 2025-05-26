using Repository.Models;

namespace Services.IServices;

public interface ITimeslotService
{
    Task<List<timeslot>> GetAllAsync();

    Task<timeslot> GetByIDAsync(int id);

    Task<List<timeslot>> SearchByStartTimeOrEndTimeAsync(TimeOnly? startTime, TimeOnly? endTime);

    Task<int> CreateTimeslot(timeslot timeslot);

    Task<int> UpdateTimeSlot(timeslot timeslot);

    Task<bool> DeleteAsync(int timeslotId);
}