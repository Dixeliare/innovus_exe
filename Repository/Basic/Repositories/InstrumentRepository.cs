using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class InstrumentRepository : GenericRepository<instrument>
{
    public InstrumentRepository()
    {
    }
    
    public InstrumentRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<instrument>> GetAllAsync()
    {
        return await _context.instruments.Include(d => d.documents).ToListAsync();
    }

    public async Task<instrument> GetByIdAsync(int id)
    {
        return await _context.instruments.Include(d => d.documents).FirstOrDefaultAsync(d => d.instrument_id == id);
    }

    public async Task<int> CreateAsync(instrument instrument)
    {
        await _context.instruments.AddAsync(instrument);
        return await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.instruments.FindAsync(id);

        if (item == null)
        {
            return false ;
        }
        
        _context.instruments.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> UpdateAsync(instrument instrument)
    {
        var item = await _context.instruments.FindAsync(instrument.instrument_id);

        if (item == null)
        {
            return 0;
        }
        
        item.instrument_name = instrument.instrument_name;
        
        return await _context.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<instrument>> SearchInstrumentsAsync(string? instrumentName = null)
    {
        IQueryable<instrument> query = _context.instruments;

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