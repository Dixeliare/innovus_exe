using Repository.Models;

namespace Services.IServices;

public interface IClassService
{
    Task<List<_class>> GetAll();
    Task<_class> GetById(int id);
    Task<int> CreateAsync(_class entity);
    Task<int> UpdateAsync(_class entity);
    Task<int> DeleteAsync(int id);
    Task<IEnumerable<_class>> SearchClassesAsync(int? instrumentId = null, string? classCode = null);
}