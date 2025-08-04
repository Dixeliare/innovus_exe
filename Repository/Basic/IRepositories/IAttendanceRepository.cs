using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IAttendanceRepository: IGenericRepository<attendance>
{
    Task<IEnumerable<attendance>> GetAllAsync();
    Task<attendance> GetByIdAsync(int id);
    // Task<attendance> AddAsync(attendance entity);
    // Task UpdateAsync(attendance entity);
    // Task<bool> DeleteAsync(int id);

    // ĐÃ SỬA: Thay đổi kiểu của tham số status thành int? statusId
    Task<IEnumerable<attendance>> SearchAttendancesAsync(
        int? statusId = null, // Đã thay đổi
        string? note = null,
        int? userId = null,
        int? classSessionId = null
    );
    // Eager loading methods
    Task<IEnumerable<attendance>> GetAllAttendancesWithDetailsAsync();
    Task<attendance?> GetAttendanceByIdWithDetailsAsync(int id);       

    // ĐÃ SỬA: Thay đổi kiểu của tham số status thành int? statusId
    Task<IEnumerable<attendance>> SearchAttendancesWithDetailsAsync(
        int? statusId = null, // Đã thay đổi
        string? note = null,
        int? userId = null,
        int? classSessionId = null
    );

    // Phương thức cụ thể để lấy điểm danh theo ClassSessionId (cho việc xóa ClassSession)
    Task<IEnumerable<attendance>> GetAttendancesByClassSessionIdAsync(int classSessionId);
    
}