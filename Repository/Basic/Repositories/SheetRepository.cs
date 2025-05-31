using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class SheetRepository : GenericRepository<sheet>
{
    private readonly AppDbContext _context;

    public SheetRepository()
    {
    }

    public SheetRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<sheet>> GetAllAsync()
    {
        return await _context.sheets
            .Include(s => s.sheet_music)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<sheet> GetByIdAsync(int id)
    {
        return await _context.sheets
            .Include(s => s.sheet_music)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.sheet_id == id);
    }

    public async Task<int> CreateAsync(sheet sheet)
    {
        await _context.sheets.AddAsync(sheet);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(sheet sheet)
    {
        var item = await _context.sheets.FindAsync(sheet.sheet_id);

        if (item == null)
        {
            return 0;
        }
        
        item.sheet_url = sheet.sheet_url;
        return await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.sheets.FindAsync(id);

        if (item == null)
        {
            return false;
        }
        
        _context.sheets.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }
    
    // Không cần hàm search cho sheet
}