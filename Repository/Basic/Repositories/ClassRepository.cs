using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class ClassRepository : GenericRepository<_class>
{
    public ClassRepository()
    {
    }
    
    public ClassRepository(AppDbContext context) => _context = context;

    public async Task<List<_class>> GetAll()
    {
        return await _context._classes
            .Include(c => c.class_sessions)
            .Include(u => u.users)
            .ToListAsync();
    }

    public async Task<_class> GetById(int id)
    {
        return await _context._classes
            .Include(c => c.class_sessions)
            .Include(u => u.users)
            .FirstOrDefaultAsync(c => c.class_id == id);
    }

    public async Task<int> CreateAsync(_class entity)
    {
        await _context._classes.AddAsync(entity);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(_class entity)
    {
        var item = await _context._classes.FirstOrDefaultAsync(c => c.class_id == entity.class_id);

        if (item == null)
        {
            return 0;
        }
        
        item.instrument_id = entity.instrument_id;
        item.class_code = entity.class_code;
        
        return await _context.SaveChangesAsync();
    }
    
    public async Task<int> DeleteAsync(int id)
    {
        var item = await _context._classes.FindAsync(id);
        if (item == null)
        {
            return 0;
        }
        _context._classes.Remove(item);
        return await _context.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<_class>> SearchClassesAsync(int? instrumentId = null, string? classCode = null)
    {
        IQueryable<_class> query = _context._classes;

        // Kiểm tra xem có bất kỳ tham số tìm kiếm nào được cung cấp không
        if (instrumentId.HasValue || !string.IsNullOrEmpty(classCode))
        {
            // Áp dụng điều kiện WHERE với logic OR
            query = query.Where(c =>
                    (instrumentId.HasValue && c.instrument_id == instrumentId.Value) ||
                    (!string.IsNullOrEmpty(classCode) && EF.Functions.ILike(c.class_code, $"%{classCode}%")) // Dùng ILike cho tìm kiếm không phân biệt hoa thường và partial match
            );
        }
        // Nếu cả hai tham số đều là null/empty, query sẽ không bị lọc và trả về tất cả.

        // Bạn có thể thêm `.Include()` nếu muốn eager load các navigation properties
        // Ví dụ: .Include(c => c.instrument) nếu bạn có navigation property đến bảng instrument

        return await query.ToListAsync();
    }
}