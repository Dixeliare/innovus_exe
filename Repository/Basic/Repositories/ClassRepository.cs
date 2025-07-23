using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class ClassRepository : GenericRepository<_class>, IClassRepository
{
    private readonly AppDbContext _context; 
    
    public ClassRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
    

    public async Task<IEnumerable<_class>> GetAll()
    {
        return await _dbSet
            .Include(c => c.class_sessions)
            .Include(u => u.users)
            .Include(i => i.instrument)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<_class> GetById(int id)
    {
        return await _dbSet
            .Include(c => c.class_sessions)
            .Include(u => u.users)
            .Include(i => i.instrument)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.class_id == id);
    }

    // public async Task<_class> AddAsync(_class entity)
    // {
    //     _context._classes.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(_class entity)
    // {
    //     _context._classes.Update(entity);
    //     await _context.SaveChangesAsync();
    // }
    //
    // public async Task<bool> DeleteAsync(int id)
    // {
    //     var item = await _context._classes.FindAsync(id);
    //     if (item == null)
    //     {
    //         return false;
    //     }
    //     _context._classes.Remove(item);
    //     return await _context.SaveChangesAsync() > 0;
    // }
    
    public async Task<IEnumerable<_class>> SearchClassesAsync(int? instrumentId = null, string? classCode = null)
    {
        IQueryable<_class> query = _dbSet;

        if (instrumentId.HasValue || !string.IsNullOrEmpty(classCode))
        {
            query = query.Where(c =>
                (instrumentId.HasValue && c.instrument_id == instrumentId.Value) ||
                (!string.IsNullOrEmpty(classCode) && EF.Functions.ILike(c.class_code, $"%{classCode}%"))
            );
        }
        
        // Đảm bảo Instrument được tải khi tìm kiếm để Service có thể ánh xạ tên nhạc cụ
        query = query.Include(i => i.instrument);

        return await query.ToListAsync();
    }

    
    // THÊM PHƯƠNG THỨC NÀY NẾU CHƯA CÓ HOẶC CẦN CHỈNH SỬA
    public async Task<_class?> GetClassWithUsersAsync(int classId)
    {
        return await _dbSet
            .Include(c => c.instrument)
            .Include(c => c.users)
            .ThenInclude(u => u.role)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.class_id == classId);
    }
    
    // SỬA LỖI Ở ĐÂY: DÙNG _dbSet thay vì _dbSetcontext.Classes hoặc _context.Classes
    public async Task<_class?> GetByIdWithDetails(int id)
    {
        return await _dbSet // Đã sửa
            .Include(c => c.instrument)
            .Include(c => c.class_sessions)
            .ThenInclude(cs => cs.day)
            .Include(c => c.class_sessions)
            .ThenInclude(cs => cs.time_slot)
            .Include(c => c.users)
            .FirstOrDefaultAsync(c => c.class_id == id);
    }

    // SỬA LỖI Ở ĐÂY: DÙNG _context.Users để truy cập DbSet của User
    public async Task<IEnumerable<_class>> GetClassesByUserId(int userId)
    {
        return await _context.users // Đã sửa
            .Where(u => u.user_id == userId)
            .SelectMany(u => u.classes)
            .Include(c => c.instrument)
            .Include(c => c.class_sessions)
            .ThenInclude(cs => cs.day)
            .Include(c => c.class_sessions)
            .ThenInclude(cs => cs.time_slot)
            .ToListAsync();
    }

    // SỬA LỖI Ở ĐÂY: DÙNG _dbSet thay vì _context.Classes
    public async Task<IEnumerable<_class>> GetAllWithDetails()
    {
        return await _dbSet // Đã sửa
            .Include(c => c.instrument)
            .Include(c => c.class_sessions)
            .ThenInclude(cs => cs.day)
            .Include(c => c.class_sessions)
            .ThenInclude(cs => cs.time_slot)
            .Include(c => c.users)
            .ToListAsync();
    }
    
    public async Task<_class?> GetClassWithSessionsAndTimeSlotsAndDayAndWeekAndInstrumentAsync(int classId)
    {
        return await _dbSet
            .Include(c => c.instrument) // Include instrument for the class
            .Include(c => c.class_sessions) // Include class sessions
            .ThenInclude(cs => cs.day) // Then include the Day for each ClassSession
            .ThenInclude(d => d.week) // And the Week for each Day
            .Include(c => c.class_sessions) // Re-include to branch for TimeSlot
            .ThenInclude(cs => cs.time_slot) // Then include the TimeSlot for each ClassSession
            .FirstOrDefaultAsync(c => c.class_id == classId);
    }
}