using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class RoleRepository : GenericRepository<role>, IRoleRepository
{
    public RoleRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<role>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<role?> GetByIdAsync(int id)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(r => r.role_id == id);
    }

    // public async Task<role> AddAsync(role entity)
    // {
    //     _context.roles.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(role entity)
    // {
    //     _context.roles.Update(entity);
    //     await _context.SaveChangesAsync();
    // }
    //
    // public async Task DeleteAsync(int id)
    // {
    //     var role = await _context.roles.FindAsync(id);
    //     if (role != null)
    //     {
    //         _context.roles.Remove(role);
    //         await _context.SaveChangesAsync();
    //     }
    // }
}