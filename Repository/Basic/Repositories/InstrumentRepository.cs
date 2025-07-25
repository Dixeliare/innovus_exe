using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class InstrumentRepository : GenericRepository<instrument>, IInstrumentRepository
{
    public InstrumentRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<instrument>> GetAllAsync()
    {
        return await _dbSet
            .Include(d => d.documents)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<instrument> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(d => d.documents)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.instrument_id == id);
    }
    
    public async Task<IEnumerable<instrument>> SearchInstrumentsAsync(string? instrumentName = null)
    {
        IQueryable<instrument> query = _dbSet;

        // Áp dụng điều kiện tìm kiếm nếu instrumentName được cung cấp
        if (!string.IsNullOrEmpty(instrumentName))
        {
            // Sử dụng EF.Functions.ILike cho tìm kiếm không phân biệt chữ hoa/thường và khớp một phần
            query = query.Where(i => EF.Functions.ILike(i.instrument_name, $"%{instrumentName}%"));
        }
        // Nếu instrumentName là null hoặc rỗng, query sẽ không bị lọc và trả về tất cả.

        // Bạn có thể thêm .Include() nếu muốn eager load các navigation properties
        // Ví dụ: .Include(i => i.documents)

        return await query.ToListAsync();
    }
}