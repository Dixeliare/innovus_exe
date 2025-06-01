using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IInstrumentService
{
    Task<IEnumerable<instrument>> GetAllAsync();
    Task<instrument> GetByIdAsync(int id);
    Task<InstrumentDto> AddAsync(CreateInstrumentDto createInstrumentDto);
    Task UpdateAsync(UpdateInstrumentDto updateInstrumentDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<instrument>> SearchInstrumentsAsync(string? instrumentName = null);
}