using DTOs;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class WeekService : IWeekService
{
    private readonly IWeekRepository _weekRepository;
    private readonly IScheduleRepository _scheduleRepository; // Để kiểm tra khóa ngoại

    public WeekService(IWeekRepository weekRepository, IScheduleRepository scheduleRepository)
    {
        _weekRepository = weekRepository;
        _scheduleRepository = scheduleRepository;
    }
    
    public async Task<IEnumerable<week>> GetAll()
    {
        return await _weekRepository.GetAllAsync();
    }

    public async Task<week> GetById(int id)
    {
        return await _weekRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<WeekDto>> GetWeeksByScheduleIdAsync(int scheduleId)
        {
            var weeks = await _weekRepository.GetWeeksByScheduleIdAsync(scheduleId);
            return weeks.Select(MapToWeekDto);
        }

        // CREATE Week
        public async Task<WeekDto> AddAsync(CreateWeekDto createWeekDto)
        {
            // Kiểm tra khóa ngoại Schedule
            var scheduleExists = await _scheduleRepository.GetByIDAsync(createWeekDto.ScheduleId);
            if (scheduleExists == null)
            {
                throw new KeyNotFoundException($"Schedule with ID {createWeekDto.ScheduleId} not found.");
            }

            var weekEntity = new week
            {
                week_number = createWeekDto.WeekNumber,
                day_of_week = createWeekDto.DayOfWeek,
                schedule_id = createWeekDto.ScheduleId
            };

            var addedWeek = await _weekRepository.AddAsync(weekEntity);
            return MapToWeekDto(addedWeek);
        }

        // UPDATE Week
        public async Task UpdateAsync(UpdateWeekDto updateWeekDto)
        {
            var existingWeek = await _weekRepository.GetByIdAsync(updateWeekDto.WeekId);

            if (existingWeek == null)
            {
                throw new KeyNotFoundException($"Week with ID {updateWeekDto.WeekId} not found.");
            }

            // Cập nhật các trường nếu có giá trị được cung cấp
            if (updateWeekDto.WeekNumber.HasValue)
            {
                existingWeek.week_number = updateWeekDto.WeekNumber.Value;
            }
            if (updateWeekDto.DayOfWeek.HasValue)
            {
                existingWeek.day_of_week = updateWeekDto.DayOfWeek.Value;
            }

            // Cập nhật ScheduleId nếu được cung cấp và khác với giá trị hiện tại
            if (updateWeekDto.ScheduleId.HasValue && existingWeek.schedule_id != updateWeekDto.ScheduleId.Value)
            {
                var scheduleExists = await _scheduleRepository.GetByIDAsync(updateWeekDto.ScheduleId.Value);
                if (scheduleExists == null)
                {
                    throw new KeyNotFoundException($"Schedule with ID {updateWeekDto.ScheduleId} not found for update.");
                }
                existingWeek.schedule_id = updateWeekDto.ScheduleId.Value;
            }

            await _weekRepository.UpdateAsync(existingWeek);
        }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _weekRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<week>> SearchWeeksAsync(DateOnly? dayOfWeek, int? scheduleId)
    {
        return await _weekRepository.SearchWeeksAsync(dayOfWeek, scheduleId);
    }
    
    private WeekDto MapToWeekDto(week model)
    {
        return new WeekDto
        {
            WeekId = model.week_id,
            WeekNumber = model.week_number,
            DayOfWeek = model.day_of_week,
            ScheduleId = model.schedule_id
        };
    }
}