using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IAttendanceService
{
    Task<IEnumerable<attendance>> GetAllAsync();
    Task<attendance> GetByIdAsync(int id);
    Task<AttendanceDto> AddAsync(CreateAttendanceDto createAttendanceDto);
    Task UpdateAsync(UpdateAttendanceDto updateAttendanceDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<attendance>> SearchAttendancesAsync(bool? status = null, string? note = null);

}