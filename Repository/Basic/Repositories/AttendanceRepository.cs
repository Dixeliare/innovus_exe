using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class AttendanceRepository : GenericRepository<attendance>, IAttendanceRepository
{
    public AttendanceRepository()
    {
    }
    
    public AttendanceRepository(AppDbContext context) => _context = context;
    
    public async Task<IEnumerable<attendance>> GetAllAsync()
    {
        return await _context.attendances
            .Include(c => c.class_session)
            .Include(u => u.user)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<attendance> GetByIdAsync(int id)
    {
        return await _context.attendances
            .Include(c => c.class_session)
            .Include(u => u.user)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.attendance_id == id);
    }

    public async Task<attendance> AddAsync(attendance entity)
    {
        _context.attendances.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(attendance entity)
    {
        _context.attendances.Update(entity);
        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.attendances.FindAsync(id);
        if (item == null)
        {
            return false;
        }
        _context.attendances.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<IEnumerable<attendance>> SearchAttendancesAsync(bool? status = null, string? note = null)
    {
        IQueryable<attendance> query = _context.attendances;

        // Kiểm tra xem có bất kỳ tham số tìm kiếm nào được cung cấp không
        if (status.HasValue || !string.IsNullOrEmpty(note))
        {
            // Áp dụng điều kiện WHERE với logic OR
            query = query.Where(a =>
                    (status.HasValue && a.status == status.Value) || // Khớp theo trạng thái
                    (!string.IsNullOrEmpty(note) && EF.Functions.ILike(a.note, $"%{note}%")) // HOẶC khớp theo ghi chú (partial, case-insensitive)
            );
        }
        // Nếu cả hai tham số đều là null/empty, query sẽ không bị lọc và trả về tất cả.

        // Bạn có thể thêm `.Include()` nếu muốn eager load các navigation properties
        // Ví dụ: .Include(a => a.user).Include(a => a.class_session)
        // để lấy thông tin của user và class_session cùng lúc.

        return await query.ToListAsync();
    }
}