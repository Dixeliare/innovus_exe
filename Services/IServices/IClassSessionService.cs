using Repository.Models;

namespace Services.IServices;

public interface IClassSessionService
{
    Task<IEnumerable<class_session>> GetAll();
    Task<class_session> GetById(int id);
    Task<int> CreateAsync(class_session item);
    Task<int> UpdateAsync(class_session item);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<class_session>> SearchClassSessionsAsync(
        DateOnly? date = null,
        string? roomCode = null,
        int? weekId = null,
        int? classId = null,
        int? timeSlotId = null);
}