using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class ClassSessionRepository : GenericRepository<class_session>, IClassSessionRepository
{
    public ClassSessionRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<class_session>> GetAll()
    {
        var items = await _dbSet
            .Include(c => c._class)
            .Include(a => a.attendances)
            .Include(t => t.time_slot)
            .Include(w => w.week)
            .AsSplitQuery()
            .ToListAsync();
        
        return items ?? new List<class_session>();
    }

    public async Task<class_session> GetByIdAsync(int id)
    {
        var item = await _dbSet
            .Include(c => c._class)
            .Include(a => a.attendances)
            .Include(t => t.time_slot)
            .Include(w => w.week)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.class_session_id == id);
        return item ?? new class_session();
    }

    // public async Task<class_session> AddAsync(class_session entity)
    // {
    //     _context.class_sessions.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(class_session entity)
    // {
    //     _context.class_sessions.Update(entity);
    //     await _context.SaveChangesAsync();
    // }
    //
    // public async Task<bool> DeleteAsync(int id)
    // {
    //     var item = await _context.class_sessions.FindAsync(id);
    //
    //     if (item == null)
    //     {
    //         return false;
    //     }
    //     
    //     _context.class_sessions.Remove(item);
    //     return await _context.SaveChangesAsync() > 0;
    // }
    
    public async Task<IEnumerable<class_session>> SearchClassSessionsAsync(
        DateOnly? date = null,
        string? roomCode = null,
        int? weekId = null,
        int? classId = null,
        int? timeSlotId = null)
    {
        IQueryable<class_session> query = _dbSet;

        // Kiểm tra xem có bất kỳ tham số tìm kiếm nào được cung cấp không
        if (date.HasValue ||
            !string.IsNullOrEmpty(roomCode) ||
            weekId.HasValue ||
            classId.HasValue ||
            timeSlotId.HasValue)
        {
            // Áp dụng điều kiện WHERE với logic OR
            query = query.Where(cs =>
                (date.HasValue && cs.date == date.Value) ||
                (!string.IsNullOrEmpty(roomCode) && EF.Functions.ILike(cs.room_code, $"%{roomCode}%")) || // Sử dụng ILike cho tìm kiếm không phân biệt hoa thường và partial match
                (weekId.HasValue && cs.week_id == weekId.Value) ||
                (classId.HasValue && cs.class_id == classId.Value) ||
                (timeSlotId.HasValue && cs.time_slot_id == timeSlotId.Value)
            );
        }
        // Nếu tất cả các tham số đều là null/empty, query sẽ không bị lọc và trả về tất cả.

        // Bạn có thể thêm `.Include()` nếu muốn eager load các navigation properties
        // Ví dụ: .Include(cs => cs.week).Include(cs => cs.time_slot)
        // để lấy thông tin của week và timeslot cùng lúc.

        return await query.ToListAsync();
    }
}