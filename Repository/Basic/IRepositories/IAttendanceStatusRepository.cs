using Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Basic.IRepositories
{
    public interface IAttendanceStatusRepository: IGenericRepository<attendance_status>
    {
        Task<IEnumerable<attendance_status>> GetAllAsync();
        Task<attendance_status?> GetByIdAsync(int id);
        Task UpdateAsync(attendance_status entity);
    }
} 