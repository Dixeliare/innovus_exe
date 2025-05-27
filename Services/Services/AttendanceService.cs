using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AttendanceRepository _attendanceService;
    
    public AttendanceService(AttendanceRepository attendanceService) => _attendanceService = attendanceService;
    
    public async Task<IEnumerable<attendance>> GetAllAsync()
    {
        return await _attendanceService.GetAllAsync();
    }

    public async Task<attendance> GetByIdAsync(int id)
    {
        return await _attendanceService.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(attendance attendanceEntity)
    {
        return await _attendanceService.CreateAsync(attendanceEntity);
    }

    public async Task<int> UpdateAsync(attendance attendanceEntity)
    {
        return await _attendanceService.UpdateAsync(attendanceEntity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _attendanceService.DeleteAsync(id);
    }

    public async Task<IEnumerable<attendance>> SearchAttendancesAsync(bool? status = null, string? note = null)
    {
        return await _attendanceService.SearchAttendancesAsync(status, note);
    }
}