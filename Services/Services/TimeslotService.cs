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

public class TimeslotService : ITimeslotService
{
    // private readonly ITimeslotRepository _timeslotRepository;
    //
    // public TimeslotService(ITimeslotRepository timeslotRepository) => _timeslotRepository = timeslotRepository;
    
    private readonly IUnitOfWork _unitOfWork;

    public TimeslotService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TimeslotDto>> GetAllAsync()
    {
        var timeslots = await _unitOfWork.Timeslots.GetAllAsync();
        // Ánh xạ các timeslot entities sang TimeslotDto trước khi trả về
        return timeslots.Select(MapToTimeslotDto);
    }

    public async Task<TimeslotDto> GetByIDAsync(int id)
    {
        var timeslot = await _unitOfWork.Timeslots.GetByIdAsync(id);
        if (timeslot == null)
        {
            throw new NotFoundException("Timeslot", "Id", id);
        }
        return MapToTimeslotDto(timeslot);
    }

    public async Task<IEnumerable<timeslot>> SearchByStartTimeOrEndTimeAsync(TimeOnly? startTime, TimeOnly? endTime)
    {
        return await _unitOfWork.Timeslots.SearchTimeslotsAsync(startTime, endTime);
    }

    public async Task<TimeslotDto> AddAsync(CreateTimeslotDto createTimeslotDto)
    {
        // Thêm validation nghiệp vụ: EndTime phải sau StartTime
        if (createTimeslotDto.EndTime <= createTimeslotDto.StartTime)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "EndTime", new string[] { "Thời gian kết thúc phải sau thời gian bắt đầu." } }
            });
        }

        var timeslotEntity = new timeslot
        {
            start_time = createTimeslotDto.StartTime,
            end_time = createTimeslotDto.EndTime
        };

        try
        {
            var addedTimeslot = await _unitOfWork.Timeslots.AddAsync(timeslotEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
            return MapToTimeslotDto(addedTimeslot);
        }
        catch (DbUpdateException dbEx) // Bắt lỗi từ Entity Framework (ví dụ: lỗi ràng buộc)
        {
            // Có thể kiểm tra các lỗi cụ thể từ dbEx.InnerException nếu cần thiết
            throw new ApiException("Có lỗi xảy ra khi thêm khung thời gian vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex) // Bắt các lỗi không mong muốn khác
        {
            throw new ApiException("An unexpected error occurred while adding the timeslot.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // UPDATE Timeslot
    public async Task UpdateAsync(UpdateTimeslotDto updateTimeslotDto)
    {
        var existingTimeslot = await _unitOfWork.Timeslots.GetByIdAsync(updateTimeslotDto.TimeslotId);

        if (existingTimeslot == null)
        {
            throw new NotFoundException("Timeslot", "Id", updateTimeslotDto.TimeslotId);
        }

        // Cập nhật các trường nếu có giá trị được cung cấp
        if (updateTimeslotDto.StartTime.HasValue)
        {
            existingTimeslot.start_time = updateTimeslotDto.StartTime.Value;
        }
        if (updateTimeslotDto.EndTime.HasValue)
        {
            existingTimeslot.end_time = updateTimeslotDto.EndTime.Value;
        }

        // Kiểm tra lại logic thời gian sau khi cập nhật
        if (existingTimeslot.end_time <= existingTimeslot.start_time)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "TimeRange", new string[] { "Thời gian kết thúc sau khi cập nhật phải sau thời gian bắt đầu." } }
            });
        }

        try
        {
            await _unitOfWork.Timeslots.UpdateAsync(existingTimeslot);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật khung thời gian trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the timeslot.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int timeslotId)
    {
        var timeslotToDelete = await _unitOfWork.Timeslots.GetByIdAsync(timeslotId);
        if (timeslotToDelete == null)
        {
            throw new NotFoundException("Timeslot", "Id", timeslotId);
        }

        try
        {
            await _unitOfWork.Timeslots.DeleteAsync(timeslotId); // Giả định DeleteAsync trong GenericRepository có thể xóa bằng ID
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
        }
        catch (DbUpdateException dbEx) // Ví dụ: lỗi ràng buộc khóa ngoại nếu timeslot đang được sử dụng ở nơi khác
        {
            throw new ApiException("Không thể xóa khung thời gian do lỗi cơ sở dữ liệu (ví dụ: đang được sử dụng).", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the timeslot.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }
    
    private TimeslotDto MapToTimeslotDto(timeslot model)
    {
        return new TimeslotDto
        {
            TimeslotId = model.timeslot_id,
            StartTime = model.start_time,
            EndTime = model.end_time
        };
    }
}