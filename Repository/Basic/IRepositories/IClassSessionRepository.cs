using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IClassSessionRepository
{
    Task<IEnumerable<class_session>> GetAll();
    Task<class_session> GetById(int id);
    Task<class_session> AddAsync(class_session entity);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<class_session>> SearchClassSessionsAsync(
        DateOnly? date = null,
        string? roomCode = null,
        int? weekId = null,
        int? classId = null,
        int? timeSlotId = null);
}