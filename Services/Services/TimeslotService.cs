using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class TimeslotService : ITimeslotService
{
    private readonly TimeslotRepository _timeslotRepository;
    
    public TimeslotService(TimeslotRepository timeslotRepository) => _timeslotRepository = timeslotRepository;
    
    public async Task<IEnumerable<timeslot>> GetAllAsync()
    {
        return await _timeslotRepository.GetAllAsync();
    }

    public async Task<timeslot> GetByIDAsync(int id)
    {
        return await _timeslotRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<timeslot>> SearchByStartTimeOrEndTimeAsync(TimeOnly? startTime, TimeOnly? endTime)
    {
        return await _timeslotRepository.SearchTimeslotsAsync(startTime, endTime);
    }

    public async Task<TimeslotDto> AddAsync(CreateTimeslotDto createTimeslotDto)
    {
        // Thêm validation: EndTime phải sau StartTime
        if (createTimeslotDto.EndTime <= createTimeslotDto.StartTime)
        {
            throw new ArgumentException("End Time must be after Start Time.");
        }

        var timeslotEntity = new timeslot
        {
            start_time = createTimeslotDto.StartTime,
            end_time = createTimeslotDto.EndTime
        };

        var addedTimeslot = await _timeslotRepository.AddAsync(timeslotEntity);
        return MapToTimeslotDto(addedTimeslot);
    }

    // UPDATE Timeslot
    public async Task UpdateAsync(UpdateTimeslotDto updateTimeslotDto)
    {
        var existingTimeslot = await _timeslotRepository.GetByIdAsync(updateTimeslotDto.TimeslotId);

        if (existingTimeslot == null)
        {
            throw new KeyNotFoundException($"Timeslot with ID {updateTimeslotDto.TimeslotId} not found.");
        }

        // Cập nhật các trường nếu có giá trị được cung cấp
        if (updateTimeslotDto.StartTime.HasValue)
        {
            existingTimeslot.start_time = updateTimeslotDto.StartTime.Value;
        }
        if (updateTimeslotDto.EndTime.HasValue)
        {
            existingTimeslot.end_time = updateTimeslotDto.EndTime.Value;
        }

        // Kiểm tra lại logic thời gian sau khi cập nhật (nếu cả hai đều được cập nhật hoặc một trong số đó)
        if (existingTimeslot.end_time <= existingTimeslot.start_time)
        {
            throw new ArgumentException("Updated End Time must be after updated Start Time.");
        }

        await _timeslotRepository.UpdateAsync(existingTimeslot);
    }

    public async Task<bool> DeleteAsync(int timeslotId)
    {
        return await _timeslotRepository.DeleteAsync(timeslotId);
    }
    
    private TimeslotDto MapToTimeslotDto(timeslot model)
    {
        return new TimeslotDto
        {
            TimeslotId = model.timeslot_id,
            StartTime = model.start_time,
            EndTime = model.end_time
        };
    }
}