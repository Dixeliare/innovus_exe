using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class WeekService : IWeekService
{
    private readonly WeekRepository _repository;
    
    public WeekService(WeekRepository repository) => _repository = repository;
    
    public async Task<IEnumerable<week>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<week> GetById(int id)
    {
        return await _repository.GetById(id);
    }

    public async Task<int> CreateAsync(week week)
    {
        return await _repository.CreateAsync(week);
    }

    public async Task<int> UpdateAsync(week week)
    {
        return await _repository.UpdateAsync(week);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<week>> SearchWeeksAsync(DateOnly? dayOfWeek, int? scheduleId)
    {
        return await _repository.SearchWeeksAsync(dayOfWeek, scheduleId);
    }
}