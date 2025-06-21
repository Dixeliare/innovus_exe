using DTOs;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class InstrumentService : IInstrumentService
{
    private readonly IInstrumentRepository _instrumentRepository;
    
    public InstrumentService(IInstrumentRepository instrumentRepository) => _instrumentRepository = instrumentRepository;
    
    public async Task<IEnumerable<instrument>> GetAllAsync()
    {
        return await _instrumentRepository.GetAllAsync();
    }

    public async Task<instrument> GetByIdAsync(int id)
    {
        return await _instrumentRepository.GetByIdAsync(id);
    }

    public async Task<InstrumentDto> AddAsync(CreateInstrumentDto createInstrumentDto)
    {
        var instrumentEntity = new instrument
        {
            instrument_name = createInstrumentDto.InstrumentName
        };

        var addedInstrument = await _instrumentRepository.AddAsync(instrumentEntity);
        return MapToInstrumentDto(addedInstrument);
    }

    // UPDATE Instrument
    public async Task UpdateAsync(UpdateInstrumentDto updateInstrumentDto)
    {
        var existingInstrument = await _instrumentRepository.GetByIdAsync(updateInstrumentDto.InstrumentId);

        if (existingInstrument == null)
        {
            throw new KeyNotFoundException($"Instrument with ID {updateInstrumentDto.InstrumentId} not found.");
        }

        // Cập nhật tên nếu có giá trị được cung cấp
        if (!string.IsNullOrEmpty(updateInstrumentDto.InstrumentName))
        {
            existingInstrument.instrument_name = updateInstrumentDto.InstrumentName;
        }
        // Nếu bạn muốn cho phép gán null cho tên nhạc cụ (nếu DB cho phép), bạn có thể thêm:
        // else if (updateInstrumentDto.InstrumentName == null)
        // {
        //     existingInstrument.instrument_name = null;
        // }

        await _instrumentRepository.UpdateAsync(existingInstrument);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _instrumentRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<instrument>> SearchInstrumentsAsync(string? instrumentName = null)
    {
        return await _instrumentRepository.SearchInstrumentsAsync(instrumentName);
    }
    
    private InstrumentDto MapToInstrumentDto(instrument model)
    {
        return new InstrumentDto
        {
            InstrumentId = model.instrument_id,
            InstrumentName = model.instrument_name
        };
    }
}