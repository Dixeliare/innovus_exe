using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class StatisticRepository : GenericRepository<statistic>, IStatisticRepository
{
    public StatisticRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<statistic>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<statistic?> GetByIdAsync(int id)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(s => s.statistic_id == id);
    }

    // public async Task<statistic> AddAsync(statistic entity)
    // {
    //     _context.statistics.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(statistic entity)
    // {
    //     _context.statistics.Update(entity);
    //     await _context.SaveChangesAsync();
    // }
    //
    // public async Task DeleteAsync(int id)
    // {
    //     var statistic = await _context.statistics.FindAsync(id);
    //     if (statistic != null)
    //     {
    //         _context.statistics.Remove(statistic);
    //         await _context.SaveChangesAsync();
    //     }
    // }
}