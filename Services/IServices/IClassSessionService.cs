using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IClassSessionService
{
    Task<IEnumerable<ClassSessionDto>> GetAllAsync();
    Task<ClassSessionDto> GetByIdAsync(int id);
    Task<ClassSessionDto> AddAsync(CreateClassSessionDto createClassSessionDto);
    Task UpdateAsync(UpdateClassSessionDto updateClassSessionDto);
    Task DeleteAsync(int id);

    Task<IEnumerable<ClassSessionDto>> SearchClassSessionsAsync(
        DateOnly? date = null,
        string? roomCode = null,
        int? weekId = null,
        int? classId = null,
        int? timeSlotId = null);
}