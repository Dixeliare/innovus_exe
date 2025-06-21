using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class SheetRepository : GenericRepository<sheet>, ISheetRepository
{
    public SheetRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<sheet>> GetAllAsync()
    {
        return await _dbSet
            .Include(s => s.sheet_music)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<sheet> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(s => s.sheet_music)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.sheet_id == id);
    }

    // public async Task<sheet> AddAsync(sheet entity)
    // {
    //     _context.sheets.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(sheet entity)
    // {
    //     _context.Entry(entity).State = EntityState.Modified;
    //     await _context.SaveChangesAsync();
    // }
    //
    // public async Task<bool> DeleteAsync(int id)
    // {
    //     var item = await _context.sheets.FindAsync(id);
    //
    //     if (item == null)
    //     {
    //         return false;
    //     }
    //     
    //     _context.sheets.Remove(item);
    //     return await _context.SaveChangesAsync() > 0;
    // }
    
}