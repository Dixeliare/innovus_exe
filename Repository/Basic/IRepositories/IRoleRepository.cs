using System.Linq.Expressions;
using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IRoleRepository: IGenericRepository<role>
{
    Task<IEnumerable<role>> GetAllAsync();
    Task<role?> GetByIdAsync(int id);

    Task<role?> FindOneAsync(Expression<Func<role, bool>> predicate);
    // Task<role> AddAsync(role entity);
    // Task UpdateAsync(role entity);
    // Task DeleteAsync(int id);
}