using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
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

    public async Task<IEnumerable<InstrumentDto>> GetAllAsync()
    {
        var instruments = await _unitOfWork.Instruments.GetAllAsync();
        return instruments.Select(MapToInstrumentDto);
    }

    public async Task<InstrumentDto> GetByIdAsync(int id)
    {
        var instrument = await _unitOfWork.Instruments.GetByIdAsync(id);
        if (instrument == null)
        {
            throw new NotFoundException("Instrument", "Id", id);
        }
        return MapToInstrumentDto(instrument);
    }

    public async Task<InstrumentDto> AddAsync(CreateInstrumentDto createInstrumentDto)
    {
        // Check for unique instrument name
        var existingInstrument = await _unitOfWork.Instruments.FindOneAsync(
            i => i.instrument_name == createInstrumentDto.InstrumentName); // Assuming FindOneAsync exists
        if (existingInstrument != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "InstrumentName", new string[] { $"Tên nhạc cụ '{createInstrumentDto.InstrumentName}' đã tồn tại." } }
            });
        }

        var instrumentEntity = new instrument
        {
            instrument_name = createInstrumentDto.InstrumentName
        };

        try
        {
            var addedInstrument = await _unitOfWork.Instruments.AddAsync(instrumentEntity);
            await _unitOfWork.CompleteAsync(); // Save changes
            return MapToInstrumentDto(addedInstrument);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm nhạc cụ vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the instrument.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // UPDATE Instrument
    public async Task UpdateAsync(UpdateInstrumentDto updateInstrumentDto)
    {
        var existingInstrument = await _unitOfWork.Instruments.GetByIdAsync(updateInstrumentDto.InstrumentId);

        if (existingInstrument == null)
        {
            throw new NotFoundException("Instrument", "Id", updateInstrumentDto.InstrumentId);
        }

        // Check for unique instrument name if the name is being updated and is different
        if (!string.IsNullOrEmpty(updateInstrumentDto.InstrumentName) && updateInstrumentDto.InstrumentName != existingInstrument.instrument_name)
        {
            var instrumentWithSameName = await _unitOfWork.Instruments.FindOneAsync(
                i => i.instrument_name == updateInstrumentDto.InstrumentName);
            if (instrumentWithSameName != null && instrumentWithSameName.instrument_id != updateInstrumentDto.InstrumentId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "InstrumentName", new string[] { $"Tên nhạc cụ '{updateInstrumentDto.InstrumentName}' đã được sử dụng bởi một nhạc cụ khác." } }
                });
            }
        }

        // Update name if a value is provided
        if (updateInstrumentDto.InstrumentName != null) // Allow null if DTO and DB allow it
        {
            existingInstrument.instrument_name = updateInstrumentDto.InstrumentName;
        }

        try
        {
            await _unitOfWork.Instruments.UpdateAsync(existingInstrument);
            await _unitOfWork.CompleteAsync(); // Save changes
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật nhạc cụ trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the instrument.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var instrumentToDelete = await _unitOfWork.Instruments.GetByIdAsync(id);
        if (instrumentToDelete == null)
        {
            throw new NotFoundException("Instrument", "Id", id);
        }

        try
        {
            await _unitOfWork.Instruments.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Save changes
        }
        catch (DbUpdateException dbEx)
        {
            // If any documents are linked to this instrument, a FK violation will occur
            throw new ApiException("Không thể xóa nhạc cụ này vì nó đang được sử dụng bởi một hoặc nhiều tài liệu.", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the instrument.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<InstrumentDto>> SearchInstrumentsAsync(string? instrumentName = null)
    {
        var instruments = await _unitOfWork.Instruments.SearchInstrumentsAsync(instrumentName); // Assuming SearchInstrumentsAsync exists
        return instruments.Select(MapToInstrumentDto);
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