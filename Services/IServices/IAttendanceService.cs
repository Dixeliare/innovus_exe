using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IAttendanceService
{
    Task<IEnumerable<AttendanceDto>> GetAllAsync();
    Task<AttendanceDto> GetByIdAsync(int id);
    Task<AttendanceDto> AddAsync(CreateAttendanceDto createAttendanceDto);
    Task UpdateAsync(UpdateAttendanceDto updateAttendanceDto);
    Task DeleteAsync(int id);
    Task<IEnumerable<AttendanceDto>> SearchAttendancesAsync(bool? status = null, string? note = null);

}