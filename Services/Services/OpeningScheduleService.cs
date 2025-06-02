using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class OpeningScheduleService : IOpeningScheduleService
{
    private readonly OpeningScheduleRepository _openingScheduleRepository;
    
    public OpeningScheduleService(OpeningScheduleRepository openingScheduleRepository) => _openingScheduleRepository = openingScheduleRepository;
    
    public async Task<IEnumerable<opening_schedule>> GetAllAsync()
    {
        return await _openingScheduleRepository.GetAllAsync();
    }

    public async Task<opening_schedule> GetByIdAsync(int id)
    {
        return await _openingScheduleRepository.GetByIdAsync(id);
    }

    public async Task<OpeningScheduleDto> AddAsync(CreateOpeningScheduleDto createOpeningScheduleDto)
        {
            var scheduleEntity = new opening_schedule
            {
                subject = createOpeningScheduleDto.Subject,
                class_code = createOpeningScheduleDto.ClassCode,
                opening_day = createOpeningScheduleDto.OpeningDay,
                end_date = createOpeningScheduleDto.EndDate,
                schedule = createOpeningScheduleDto.Schedule,
                student_quantity = createOpeningScheduleDto.StudentQuantity,
                is_advanced_class = createOpeningScheduleDto.IsAdvancedClass ?? false // Mặc định là false nếu không được cung cấp
            };

            var addedSchedule = await _openingScheduleRepository.AddAsync(scheduleEntity);
            return MapToOpeningScheduleDto(addedSchedule);
        }

        // UPDATE Opening Schedule
        public async Task UpdateAsync(UpdateOpeningScheduleDto updateOpeningScheduleDto)
        {
            var existingSchedule = await _openingScheduleRepository.GetByIdAsync(updateOpeningScheduleDto.OpeningScheduleId);

            if (existingSchedule == null)
            {
                throw new KeyNotFoundException($"Opening Schedule with ID {updateOpeningScheduleDto.OpeningScheduleId} not found.");
            }

            // Cập nhật các trường nếu có giá trị được cung cấp
            if (!string.IsNullOrEmpty(updateOpeningScheduleDto.Subject))
            {
                existingSchedule.subject = updateOpeningScheduleDto.Subject;
            }
            if (!string.IsNullOrEmpty(updateOpeningScheduleDto.ClassCode))
            {
                existingSchedule.class_code = updateOpeningScheduleDto.ClassCode;
            }
            if (updateOpeningScheduleDto.OpeningDay.HasValue)
            {
                existingSchedule.opening_day = updateOpeningScheduleDto.OpeningDay.Value;
            }
            if (updateOpeningScheduleDto.EndDate.HasValue)
            {
                existingSchedule.end_date = updateOpeningScheduleDto.EndDate.Value;
            }
            if (!string.IsNullOrEmpty(updateOpeningScheduleDto.Schedule))
            {
                existingSchedule.schedule = updateOpeningScheduleDto.Schedule;
            }
            if (updateOpeningScheduleDto.StudentQuantity.HasValue)
            {
                existingSchedule.student_quantity = updateOpeningScheduleDto.StudentQuantity.Value;
            }
            if (updateOpeningScheduleDto.IsAdvancedClass.HasValue)
            {
                existingSchedule.is_advanced_class = updateOpeningScheduleDto.IsAdvancedClass.Value;
            }

            await _openingScheduleRepository.UpdateAsync(existingSchedule);
        }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _openingScheduleRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<opening_schedule>> SearchOpeningSchedulesAsync(string? subject = null, string? classCode = null, DateOnly? openingDay = null,
        DateOnly? endDate = null, string? schedule = null, int? studentQuantity = null, bool? isAdvancedClass = null)
    {
        return await _openingScheduleRepository.SearchOpeningSchedulesAsync(subject, classCode, openingDay, endDate, schedule, studentQuantity, isAdvancedClass);
    }
    
    private OpeningScheduleDto MapToOpeningScheduleDto(opening_schedule model)
    {
        return new OpeningScheduleDto
        {
            OpeningScheduleId = model.opening_schedule_id,
            Subject = model.subject,
            ClassCode = model.class_code,
            OpeningDay = model.opening_day,
            EndDate = model.end_date,
            Schedule = model.schedule,
            StudentQuantity = model.student_quantity,
            IsAdvancedClass = model.is_advanced_class
        };
    }
}