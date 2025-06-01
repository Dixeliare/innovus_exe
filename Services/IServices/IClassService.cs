using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IClassService
{
    Task<IEnumerable<_class>> GetAll();
    Task<_class> GetById(int id);
    Task<ClassDto> AddAsync(CreateClassDto createClassDto);
    Task UpdateAsync(UpdateClassDto updateClassDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<_class>> SearchClassesAsync(int? instrumentId = null, string? classCode = null);
}