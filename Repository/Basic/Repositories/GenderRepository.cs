using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class GenderRepository: GenericRepository<gender>, IGenderRepository
{
    public GenderRepository(AppDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<gender>> GetAllAsync()
    {
        return await _dbSet
            .OrderBy(g => g.gender_name)
            .AsSplitQuery()
            .ToListAsync();
    }
}