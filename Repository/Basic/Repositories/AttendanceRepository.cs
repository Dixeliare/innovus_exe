using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class AttendanceRepository : GenericRepository<attendance>, IAttendanceRepository
{
    public AttendanceRepository(AppDbContext context) : base(context)
    {
        
    }
    
    public async Task<IEnumerable<attendance>> GetAllAsync()
    {
        return await _dbSet
            .Include(a => a.class_session)
                .ThenInclude(cs => cs._class)
            .Include(a => a.user)
            .Include(a => a.status)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<attendance> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(a => a.class_session)
                .ThenInclude(cs => cs._class)
            .Include(a => a.user)
            .Include(a => a.status)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.attendance_id == id);
    }

    public async Task<IEnumerable<attendance>> GetAllAttendancesWithDetailsAsync()
    {
        return await _dbSet
            .Include(a => a.class_session)
                .ThenInclude(cs => cs._class)
            .Include(a => a.user)
            .Include(a => a.status)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<attendance?> GetAttendanceByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.class_session)
                .ThenInclude(cs => cs._class)
            .Include(a => a.user)
            .Include(a => a.status)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.attendance_id == id);
    }

    // ĐÃ SỬA: Thay đổi kiểu tham số thành int? statusId và sử dụng a.status_id
    public async Task<IEnumerable<attendance>> SearchAttendancesWithDetailsAsync(
        int? statusId = null, // Đã thay đổi kiểu tham số
        string? note = null,
        int? userId = null,
        int? classSessionId = null)
    {
        IQueryable<attendance> query = _dbSet;

        if (statusId.HasValue) // Sử dụng statusId
        {
            query = query.Where(a => a.status_id == statusId.Value); // So sánh với status_id
        }
        if (!string.IsNullOrEmpty(note))
        {
            query = query.Where(a => EF.Functions.ILike(a.note, $"%{note}%"));
        }
        if (userId.HasValue)
        {
            query = query.Where(a => a.user_id == userId.Value);
        }
        if (classSessionId.HasValue)
        {
            query = query.Where(a => a.class_session_id == classSessionId.Value);
        }

        return await query
            .Include(a => a.class_session)
                .ThenInclude(cs => cs._class)
            .Include(a => a.user)
            .Include(a => a.status)
            .AsSplitQuery()
            .ToListAsync();
    }
    
    public async Task<IEnumerable<attendance>> GetAttendancesByClassSessionIdAsync(int classSessionId)
    {
        return await _dbSet
            .Where(a => a.class_session_id == classSessionId)
            .AsNoTracking()
            .ToListAsync();
    }

    // ĐÃ SỬA: Thay đổi kiểu tham số thành int? statusId và sử dụng a.status_id
    public async Task<IEnumerable<attendance>> SearchAttendancesAsync(
        int? statusId = null, // Đã thay đổi kiểu tham số
        string? note = null,
        int? userId = null, 
        int? classSessionId = null)
    {
        IQueryable<attendance> query = _dbSet;

        if (statusId.HasValue) // Sử dụng statusId
        {
            query = query.Where(a => a.status_id == statusId.Value); // So sánh với status_id
        }
        if (!string.IsNullOrEmpty(note))
        {
            query = query.Where(a => EF.Functions.ILike(a.note, $"%{note}%"));
        }
        if (userId.HasValue)
        {
            query = query.Where(a => a.user_id == userId.Value);
        }
        if (classSessionId.HasValue)
        {
            query = query.Where(a => a.class_session_id == classSessionId.Value);
        }
        
        return await query.ToListAsync();
    }
}