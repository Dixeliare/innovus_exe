using DTOs;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class TimeslotService : ITimeslotService
{
    // private readonly ITimeslotRepository _timeslotRepository;
    //
    // public TimeslotService(ITimeslotRepository timeslotRepository) => _timeslotRepository = timeslotRepository;
    
    private readonly IUnitOfWork _unitOfWork;

    public TimeslotService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<timeslot>> GetAllAsync()
    {
        return await _unitOfWork.Timeslots.GetAllAsync();
    }

    public async Task<timeslot> GetByIDAsync(int id)
    {
        return await _unitOfWork.Timeslots.GetByIdAsync(id);
    }

    public async Task<IEnumerable<timeslot>> SearchByStartTimeOrEndTimeAsync(TimeOnly? startTime, TimeOnly? endTime)
    {
        return await _unitOfWork.Timeslots.SearchTimeslotsAsync(startTime, endTime);
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

        var addedTimeslot = await _unitOfWork.Timeslots.AddAsync(timeslotEntity);
        return MapToTimeslotDto(addedTimeslot);
    }

    // UPDATE Timeslot
    public async Task UpdateAsync(UpdateTimeslotDto updateTimeslotDto)
    {
        var existingTimeslot = await _unitOfWork.Timeslots.GetByIdAsync(updateTimeslotDto.TimeslotId);

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

        await _unitOfWork.Timeslots.UpdateAsync(existingTimeslot);
    }

    public async Task<bool> DeleteAsync(int timeslotId)
    {
        return await _unitOfWork.Timeslots.DeleteAsync(timeslotId);
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