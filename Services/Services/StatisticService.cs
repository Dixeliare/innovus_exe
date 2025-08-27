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
            ConsultationRequestCount = model.consultation_request_count,
            TotalGuitarClass = model.total_guitar_class,
            TotalPianoClass = model.total_piano_class
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

    // GET Statistic by Month (mới)
    public async Task<StatisticDto?> GetByMonthAsync(int year, int month)
    {
        var monthStart = new DateOnly(year, month, 1);
        var statistic = await _unitOfWork.Statistics.FindOneAsync(s => s.date == monthStart);
        if (statistic == null)
        {
            throw new NotFoundException("Statistic", $"Month {month}/{year}", 0);
        }
        return MapToStatisticDto(statistic);
    }

    // GET Current Month Statistic (mới)
    public async Task<StatisticDto?> GetCurrentMonthAsync()
    {
        var currentMonthDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        var statistic = await _unitOfWork.Statistics.FindOneAsync(s => s.date == currentMonthDate);
        if (statistic == null)
        {
            // Tạo mới nếu chưa có
            await UpdateStatisticsAsync();
            statistic = await _unitOfWork.Statistics.FindOneAsync(s => s.date == currentMonthDate);
        }
        return MapToStatisticDto(statistic);
    }

    // CREATE Statistic
    public async Task<StatisticDto> AddAsync(CreateStatisticDto createStatisticDto)
    {
        // Luôn tạo theo tháng (ngày 1) thay vì theo date từ DTO
        var monthStart = createStatisticDto.Date.HasValue 
            ? new DateOnly(createStatisticDto.Date.Value.Year, createStatisticDto.Date.Value.Month, 1)
            : new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
            
        var statisticEntity = new statistic
        {
            date = monthStart, // Luôn là ngày 1 của tháng
            new_students = createStatisticDto.NewStudents ?? 0,
            monthly_revenue = createStatisticDto.MonthlyRevenue ?? 0m,
            consultation_count = createStatisticDto.ConsultationCount ?? 0,
            total_students = createStatisticDto.TotalStudents ?? 0,
            consultation_request_count = createStatisticDto.ConsultationRequestCount ?? 0,
            total_guitar_class = createStatisticDto.TotalGuitarClass ?? 0,
            total_piano_class = createStatisticDto.TotalPianoClass ?? 0
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
    
    // Method để cập nhật thống kê khi có thay đổi user
    public async Task UpdateStatisticsOnUserChangeAsync()
    {
        try
        {
            // Gọi UpdateStatisticsAsync để cập nhật toàn bộ thống kê
            await UpdateStatisticsAsync();
        }
        catch (Exception ex)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật thống kê user.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }
    
    // Method để cập nhật thống kê khi có thay đổi class
    public async Task UpdateStatisticsOnClassChangeAsync()
    {
        try
        {
            // Gọi UpdateStatisticsAsync để cập nhật toàn bộ thống kê
            await UpdateStatisticsAsync();
        }
        catch (Exception ex)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật thống kê class.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }
    
            // Method để tự động cập nhật thống kê
        public async Task UpdateStatisticsAsync()
        {
            try
            {
                // Lấy thống kê của tháng hiện tại (mỗi tháng 1 bản ghi)
                var currentMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1); // Ngày 1 của tháng
                var currentStatistic = await _unitOfWork.Statistics.FindOneAsync(s => s.date == currentMonth);
                
                if (currentStatistic == null)
                {
                    currentStatistic = new statistic
                    {
                        date = currentMonth, // Ngày 1 của tháng
                        new_students = 0,
                        monthly_revenue = 0m,
                        consultation_count = 0,
                        total_students = 0,
                        consultation_request_count = 0,
                        total_guitar_class = 0,
                        total_piano_class = 0
                    };
                    await _unitOfWork.Statistics.AddAsync(currentStatistic);
                }
            
            // Cập nhật total_students (đếm user có role student và không bị disable)
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var totalStudents = allUsers.Count(u => 
                u.role?.role_name?.ToLower().Contains("student") == true && 
                !u.is_disabled.GetValueOrDefault());
            currentStatistic.total_students = totalStudents;
            
            // Cập nhật new_students (đếm user mới tạo trong tháng hiện tại)
            var currentMonthNumber = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;
            var newStudents = allUsers.Count(u => 
                u.role?.role_name?.ToLower().Contains("student") == true && 
                !u.is_disabled.GetValueOrDefault() &&
                u.create_at.HasValue &&
                u.create_at.Value.Month == currentMonthNumber &&
                u.create_at.Value.Year == currentYear);
            currentStatistic.new_students = newStudents;
            
            // Cập nhật total_guitar_class và total_piano_class
            var allClasses = await _unitOfWork.Classes.GetAll();
            var guitarClasses = allClasses.Count(c => 
                c.instrument?.instrument_name?.ToLower().Contains("guitar") == true);
            currentStatistic.total_guitar_class = guitarClasses;
            
            var pianoClasses = allClasses.Count(c => 
                c.instrument?.instrument_name?.ToLower().Contains("piano") == true);
            currentStatistic.total_piano_class = pianoClasses;
            
            // Cập nhật consultation_count và consultation_request_count (realtime)
            var allConsultationRequests = await _unitOfWork.ConsultationRequests.GetAllAsync();
            
            // Đếm tất cả consultation requests (vì không có created_at, đếm tất cả)
            currentStatistic.consultation_request_count = allConsultationRequests.Count();
            
            // Tính consultation_count (số consultation đã hoàn thành - has_contact = true)
            var completedConsultations = allConsultationRequests.Count(cr => 
                cr.has_contact == true);
            currentStatistic.consultation_count = completedConsultations;
            
            await _unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật thống kê.", ex, (int)HttpStatusCode.InternalServerError);
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
        
        // Cập nhật các trường mới nếu được cung cấp
        if (updateStatisticDto.TotalGuitarClass.HasValue)
        {
            existingStatistic.total_guitar_class = updateStatisticDto.TotalGuitarClass.Value;
        }
        if (updateStatisticDto.TotalPianoClass.HasValue)
        {
            existingStatistic.total_piano_class = updateStatisticDto.TotalPianoClass.Value;
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