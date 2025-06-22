using DTOs;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
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
        return statistic != null ? MapToStatisticDto(statistic) : null;
    }

    // CREATE Statistic
    public async Task<StatisticDto> AddAsync(CreateStatisticDto createStatisticDto)
    {
        var statisticEntity = new statistic
        {
            // Nếu date là NOT NULL trong DB và bạn muốn tự động gán ngày hiện tại khi tạo
            // date = DateOnly.FromDateTime(DateTime.Now),
            // Còn không, giữ nguyên như DTO:
            date = createStatisticDto.Date, // Có thể null hoặc được cung cấp

            new_students = createStatisticDto.NewStudents ?? 0, // Mặc định là 0 nếu không được cung cấp
            monthly_revenue = createStatisticDto.MonthlyRevenue ?? 0m, // Mặc định là 0 nếu không được cung cấp
            consultation_count = createStatisticDto.ConsultationCount ?? 0, // Mặc định là 0
            total_students = createStatisticDto.TotalStudents ?? 0, // Mặc định là 0
            consultation_request_count = createStatisticDto.ConsultationRequestCount ?? 0 // Mặc định là 0
        };

        var addedStatistic = await _unitOfWork.Statistics.AddAsync(statisticEntity);
        return MapToStatisticDto(addedStatistic);
    }

    // UPDATE Statistic
    public async Task UpdateAsync(UpdateStatisticDto updateStatisticDto)
    {
        var existingStatistic = await _unitOfWork.Statistics.GetByIdAsync(updateStatisticDto.StatisticId);

        if (existingStatistic == null)
        {
            throw new KeyNotFoundException($"Statistic with ID {updateStatisticDto.StatisticId} not found.");
        }

        // Cập nhật các trường nếu có giá trị được cung cấp (chỉ cập nhật những trường không null)
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

        await _unitOfWork.Statistics.UpdateAsync(existingStatistic);
    }

    // DELETE Statistic
    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Statistics.DeleteAsync(id);
    }
}