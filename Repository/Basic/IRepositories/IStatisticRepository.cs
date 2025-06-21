using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IStatisticRepository: IGenericRepository<statistic>
{
    Task<IEnumerable<statistic>> GetAllAsync();
    Task<statistic?> GetByIdAsync(int id);
    // Task<statistic> AddAsync(statistic entity);
    // Task UpdateAsync(statistic entity);
    // Task DeleteAsync(int id);
    
}