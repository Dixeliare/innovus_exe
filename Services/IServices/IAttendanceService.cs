using Repository.Models;

namespace Services.IServices;

public interface IAttendanceService
{
    Task<IEnumerable<attendance>> GetAllAsync();
    Task<attendance> GetByIdAsync(int id);
    Task<int> CreateAsync(attendance attendanceEntity);
    Task<int> UpdateAsync(attendance attendanceEntity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<attendance>> SearchAttendancesAsync(bool? status = null, string? note = null);

}