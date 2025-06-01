using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IClassSessionService
{
    Task<IEnumerable<class_session>> GetAll();
    Task<ClassSessionDto> GetByIdAsync(int id);
    Task<ClassSessionDto> AddAsync(CreateClassSessionDto createClassSessionDto);
    Task UpdateAsync(UpdateClassSessionDto updateClassSessionDto);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<class_session>> SearchClassSessionsAsync(
        DateOnly? date = null,
        string? roomCode = null,
        int? weekId = null,
        int? classId = null,
        int? timeSlotId = null);
}