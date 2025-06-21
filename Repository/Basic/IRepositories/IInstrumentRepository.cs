using System.Diagnostics.Metrics;
using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IInstrumentRepository: IGenericRepository<instrument>
{
    Task<IEnumerable<instrument>> GetAllAsync();
    Task<instrument> GetByIdAsync(int id);
    // Task<instrument> AddAsync(instrument entity);
    // Task UpdateAsync(instrument entity);
    // Task<bool> DeleteAsync(int id);
    Task<IEnumerable<instrument>> SearchInstrumentsAsync(string? instrumentName = null);
}