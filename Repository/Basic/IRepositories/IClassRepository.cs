using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IClassRepository
{
    Task<IEnumerable<_class>> GetAll();
    Task<_class> GetById(int id);
    Task<_class> AddAsync(_class entity);
    Task UpdateAsync(_class entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<_class>> SearchClassesAsync(int? instrumentId = null, string? classCode = null);
}