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

public class StatisticService : IStatisticService
{
    // private readonly IStatisticRepository _statisticRepository;
    //
    // public StatisticService(IStatisticRepository statisticRepository)
    // {
    //     _statisticRepository = statisticRepository;
    // }
    
    private readonly IUnitOfWork _unitOfWork;

    public StatisticService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Mapper từ Model sang DTO
    private StatisticDto MapToStatisticDto(statistic model)
    {
        return new StatisticDto
        {
            StatisticId = model.statistic_id,
            Date = model.date,
            NewStudents = model.new_students,
            MonthlyRevenue = model.monthly_revenue,
            ConsultationCount = model.consultation_count,
            TotalStudents = model.total_students,
            ConsultationRequestCount = model.consultation_request_count
        };
    }

    // GET All Statistics
    public async Task<IEnumerable<StatisticDto>> GetAllAsync()
    {
        var statistics = await _unitOfWork.Statistics.GetAllAsync();
        return statistics.Select(MapToStatisticDto);
    }

    // GET Statistic by ID
    public async Task<StatisticDto?> GetByIdAsync(int id)
    {
        var statistic = await _unitOfWork.Statistics.GetByIdAsync(id);
        if (statistic == null)
        {
            throw new NotFoundException("Statistic", "Id", id);
        }
        return MapToStatisticDto(statistic);
    }

    // CREATE Statistic
    public async Task<StatisticDto> AddAsync(CreateStatisticDto createStatisticDto)
    {
        var statisticEntity = new statistic
        {
            date = createStatisticDto.Date,
            new_students = createStatisticDto.NewStudents ?? 0,
            monthly_revenue = createStatisticDto.MonthlyRevenue ?? 0m,
            consultation_count = createStatisticDto.ConsultationCount ?? 0,
            total_students = createStatisticDto.TotalStudents ?? 0,
            consultation_request_count = createStatisticDto.ConsultationRequestCount ?? 0
        };

        try
        {
            var addedStatistic = await _unitOfWork.Statistics.AddAsync(statisticEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
            return MapToStatisticDto(addedStatistic);
        }
        catch (DbUpdateException dbEx) // Bắt lỗi từ Entity Framework
        {
            throw new ApiException("Có lỗi xảy ra khi thêm thống kê vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the statistic.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // UPDATE Statistic
    public async Task UpdateAsync(UpdateStatisticDto updateStatisticDto)
    {
        var existingStatistic = await _unitOfWork.Statistics.GetByIdAsync(updateStatisticDto.StatisticId);

        if (existingStatistic == null)
        {
            throw new NotFoundException("Statistic", "Id", updateStatisticDto.StatisticId);
        }

        // Cập nhật các trường nếu có giá trị được cung cấp
        if (updateStatisticDto.Date.HasValue)
        {
            existingStatistic.date = updateStatisticDto.Date.Value;
        }
        if (updateStatisticDto.NewStudents.HasValue)
        {
            existingStatistic.new_students = updateStatisticDto.NewStudents.Value;
        }
        if (updateStatisticDto.MonthlyRevenue.HasValue)
        {
            existingStatistic.monthly_revenue = updateStatisticDto.MonthlyRevenue.Value;
        }
        if (updateStatisticDto.ConsultationCount.HasValue)
        {
            existingStatistic.consultation_count = updateStatisticDto.ConsultationCount.Value;
        }
        if (updateStatisticDto.TotalStudents.HasValue)
        {
            existingStatistic.total_students = updateStatisticDto.TotalStudents.Value;
        }
        if (updateStatisticDto.ConsultationRequestCount.HasValue)
        {
            existingStatistic.consultation_request_count = updateStatisticDto.ConsultationRequestCount.Value;
        }

        try
        {
            await _unitOfWork.Statistics.UpdateAsync(existingStatistic);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật thống kê trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the statistic.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // DELETE Statistic
    public async Task DeleteAsync(int id)
    {
        var statisticToDelete = await _unitOfWork.Statistics.GetByIdAsync(id);
        if (statisticToDelete == null)
        {
            throw new NotFoundException("Statistic", "Id", id);
        }

        try
        {
            await _unitOfWork.Statistics.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Không thể xóa thống kê do lỗi cơ sở dữ liệu (ví dụ: đang được tham chiếu bởi user hoặc consultation_request).", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the statistic.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }
}