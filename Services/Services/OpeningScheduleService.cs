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

    public async Task<int> CreateAsync(opening_schedule opening_schedule)
    {
        return await _openingScheduleRepository.CreateAsync(opening_schedule);
    }

    public async Task<int> UpdateAsync(opening_schedule opening_schedule)
    {
        return await _openingScheduleRepository.UpdateAsync(opening_schedule);
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
}