using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IClassService
{
    Task<IEnumerable<ClassDto>> GetAllAsync();
    Task<ClassDto> GetByIdAsync(int id);
    Task<ClassDto> AddAsync(CreateClassDto createClassDto);
    Task UpdateAsync(UpdateClassDto updateClassDto);
    Task DeleteAsync(int id);
    Task<IEnumerable<ClassDto>> SearchClassesAsync(int? instrumentId = null, string? classCode = null);
}