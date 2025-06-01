using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface ITimeslotService
{
    Task<IEnumerable<timeslot>> GetAllAsync();

    Task<timeslot> GetByIDAsync(int id);

    Task<IEnumerable<timeslot>> SearchByStartTimeOrEndTimeAsync(TimeOnly? startTime = null, TimeOnly? endTime = null);

    Task<TimeslotDto> AddAsync(CreateTimeslotDto createTimeslotDto);
    Task UpdateAsync(UpdateTimeslotDto updateTimeslotDto);

    Task<bool> DeleteAsync(int timeslotId);
}