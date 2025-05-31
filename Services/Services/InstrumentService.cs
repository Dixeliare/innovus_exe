using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class InstrumentService : IInstrumentService
{
    private readonly InstrumentRepository _instrumentRepository;
    
    public InstrumentService(InstrumentRepository instrumentRepository) => _instrumentRepository = instrumentRepository;
    
    public async Task<IEnumerable<instrument>> GetAllAsync()
    {
        return await _instrumentRepository.GetAllAsync();
    }

    public async Task<instrument> GetByIdAsync(int id)
    {
        return await _instrumentRepository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(instrument instrument)
    {
        return await _instrumentRepository.CreateAsync(instrument);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _instrumentRepository.DeleteAsync(id);
    }

    public async Task<int> UpdateAsync(instrument instrument)
    {
        return await _instrumentRepository.UpdateAsync(instrument);
    }

    public async Task<IEnumerable<instrument>> SearchInstrumentsAsync(string? instrumentName = null)
    {
        return await _instrumentRepository.SearchInstrumentsAsync(instrumentName);
    }
}