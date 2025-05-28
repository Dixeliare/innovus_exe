using Repository.Models;

namespace Services.IServices;

public interface IInstrumentService
{
    Task<IEnumerable<instrument>> GetAllAsync();
    Task<instrument> GetByIdAsync(int id);
    Task<int> CreateAsync(instrument instrument);
    Task<bool> DeleteAsync(int id);
    Task<int> UpdateAsync(instrument instrument);
    Task<IEnumerable<instrument>> SearchInstrumentsAsync(string? instrumentName = null);
}