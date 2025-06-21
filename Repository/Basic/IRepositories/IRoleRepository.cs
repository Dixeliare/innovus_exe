using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IRoleRepository
{
    Task<IEnumerable<role>> GetAllAsync();
    Task<role?> GetByIdAsync(int id);
    Task<role> AddAsync(role entity);
    Task UpdateAsync(role entity);
    Task DeleteAsync(int id);
}