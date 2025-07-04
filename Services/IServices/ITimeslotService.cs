using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface ITimeslotService
{
    Task<IEnumerable<TimeslotDto>> GetAllAsync();

    Task<TimeslotDto> GetByIDAsync(int id);

    Task<IEnumerable<timeslot>> SearchByStartTimeOrEndTimeAsync(TimeOnly? startTime = null, TimeOnly? endTime = null);

    Task<TimeslotDto> AddAsync(CreateTimeslotDto createTimeslotDto);
    Task UpdateAsync(UpdateTimeslotDto updateTimeslotDto);

    Task DeleteAsync(int timeslotId);
}