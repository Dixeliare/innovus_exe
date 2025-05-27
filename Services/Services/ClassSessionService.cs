using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class ClassSessionService : IClassSessionService
{
    private readonly ClassSessionRepository _classSessionRepository;
    
    public ClassSessionService(ClassSessionRepository classSessionRepository) => _classSessionRepository = classSessionRepository;
    
    public async Task<IEnumerable<class_session>> GetAll()
    {
        return await  _classSessionRepository.GetAllAsync();
    }

    public async Task<class_session> GetById(int id)
    {
        return await _classSessionRepository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(class_session item)
    {
        return await _classSessionRepository.CreateAsync(item);
    }

    public async Task<int> UpdateAsync(class_session item)
    {
        return await _classSessionRepository.UpdateAsync(item);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _classSessionRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<class_session>> SearchClassSessionsAsync(DateOnly? date = null, string? roomCode = null, int? weekId = null, int? classId = null,
        int? timeSlotId = null)
    {
        return await _classSessionRepository.SearchClassSessionsAsync(date, roomCode, weekId, classId, timeSlotId);
    }
}