using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class ClassRepository : GenericRepository<_class>, IClassRepository
{
    public ClassRepository(AppDbContext context) : base(context)
    {
        
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

    
    // THÊM PHƯƠNG THỨC MỚI NÀY:
    public async Task<_class?> GetClassWithUsersAsync(int classId)
    {
        return await _dbSet
            .Include(c => c.instrument)
            .Include(c => c.users) // Rất quan trọng: Bao gồm danh sách users
            .ThenInclude(u => u.role) // Và bao gồm cả vai trò của mỗi user
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.class_id == classId);
    }
}