using DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IAttendanceStatusService
    {
        Task<IEnumerable<AttendanceStatusDto>> GetAllAsync();
        Task UpdateAsync(UpdateAttendanceStatusDto dto);
    }
} 