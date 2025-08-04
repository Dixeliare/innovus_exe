using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IAttendanceService
{
    Task<IEnumerable<AttendanceDto>> GetAllAsync();
    Task<AttendanceDto> GetByIdAsync(int id);
    Task<IEnumerable<AttendanceDto>> GetAttendancesByUserIdAsync(int userId);
    Task<IEnumerable<AttendanceDto>> GetAttendancesByClassSessionIdAsync(int classSessionId);
    Task<AttendanceDto> AddAsync(CreateAttendanceDto createAttendanceDto);
    Task UpdateAsync(UpdateAttendanceDto updateAttendanceDto);
    Task DeleteAsync(int id);

    Task<IEnumerable<AttendanceDto>> SearchAttendancesAsync(int? statusId = null, string? note = null,
        int? userId = null, int? classSessionId = null);
    Task BulkUpdateAsync(BulkUpdateAttendanceDto bulkUpdateDto);
}