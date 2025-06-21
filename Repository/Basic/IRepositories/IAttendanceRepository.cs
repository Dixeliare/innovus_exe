using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IAttendanceRepository
{
    Task<IEnumerable<attendance>> GetAllAsync();
    Task<attendance> GetByIdAsync(int id);
    Task<attendance> AddAsync(attendance entity);
    Task UpdateAsync(attendance entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<attendance>> SearchAttendancesAsync(bool? status = null, string? note = null);
    
}