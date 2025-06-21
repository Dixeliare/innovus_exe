using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IInstrumentRepository
{
    Task<IEnumerable<instrument>> GetAllAsync();
    Task<instrument> GetByIdAsync(int id);
    Task<instrument> AddAsync(instrument entity);
    Task UpdateAsync(instrument entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<instrument>> SearchInstrumentsAsync(string? instrumentName = null);
}