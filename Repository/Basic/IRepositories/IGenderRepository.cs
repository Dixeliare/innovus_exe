using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IGenderRepository: IGenericRepository<gender>
{
    Task<IEnumerable<gender>> GetAllAsync();
    Task<gender> GetByIdAsync(int id);
}