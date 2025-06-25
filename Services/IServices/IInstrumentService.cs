using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IInstrumentService
{
    Task<IEnumerable<InstrumentDto>> GetAllAsync();
    Task<InstrumentDto> GetByIdAsync(int id);
    Task<InstrumentDto> AddAsync(CreateInstrumentDto createInstrumentDto);
    Task UpdateAsync(UpdateInstrumentDto updateInstrumentDto);
    Task DeleteAsync(int id);
    Task<IEnumerable<InstrumentDto>> SearchInstrumentsAsync(string? instrumentName = null);
}