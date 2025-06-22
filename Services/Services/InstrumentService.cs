using DTOs;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class InstrumentService : IInstrumentService
{
    // private readonly IInstrumentRepository _instrumentRepository;
    //
    // public InstrumentService(IInstrumentRepository instrumentRepository) => _instrumentRepository = instrumentRepository;
    
    private readonly IUnitOfWork _unitOfWork;

    public InstrumentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<instrument>> GetAllAsync()
    {
        return await _unitOfWork.Instruments.GetAllAsync();
    }

    public async Task<instrument> GetByIdAsync(int id)
    {
        return await _unitOfWork.Instruments.GetByIdAsync(id);
    }

    public async Task<InstrumentDto> AddAsync(CreateInstrumentDto createInstrumentDto)
    {
        var instrumentEntity = new instrument
        {
            instrument_name = createInstrumentDto.InstrumentName
        };

        var addedInstrument = await _unitOfWork.Instruments.AddAsync(instrumentEntity);
        return MapToInstrumentDto(addedInstrument);
    }

    // UPDATE Instrument
    public async Task UpdateAsync(UpdateInstrumentDto updateInstrumentDto)
    {
        var existingInstrument = await _unitOfWork.Instruments.GetByIdAsync(updateInstrumentDto.InstrumentId);

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

        await _unitOfWork.Instruments.UpdateAsync(existingInstrument);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _unitOfWork.Instruments.DeleteAsync(id);
    }

    public async Task<IEnumerable<instrument>> SearchInstrumentsAsync(string? instrumentName = null)
    {
        return await _unitOfWork.Instruments.SearchInstrumentsAsync(instrumentName);
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