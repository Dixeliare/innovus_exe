using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class TimeslotService : ITimeslotService
{
    private readonly TimeslotRepository _timeslotRepository;
    
    public TimeslotService(TimeslotRepository timeslotRepository) => _timeslotRepository = timeslotRepository;
    
    public async Task<List<timeslot>> GetAllAsync()
    {
        return await _timeslotRepository.GetAllAsync();
    }

    public async Task<timeslot> GetByIDAsync(int id)
    {
        return await _timeslotRepository.GetByIdAsync(id);
    }

    public async Task<List<timeslot>> SearchByStartTimeOrEndTimeAsync(TimeOnly? startTime, TimeOnly? endTime)
    {
        return await _timeslotRepository.SearchTimeslotsAsync(startTime, endTime);
    }

    public async Task<int> CreateTimeslot(timeslot timeslot)
    {
        return await _timeslotRepository.CreateAsync(timeslot);
    }

    public async Task<int> UpdateTimeSlot(timeslot timeslot)
    {
        return await _timeslotRepository.UpdateAsync(timeslot);
    }

    public async Task<bool> DeleteAsync(int timeslotId)
    {
        return await _timeslotRepository.DeleteAsync(timeslotId);
    }
}