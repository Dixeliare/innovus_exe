using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Repository.Data;

namespace Repository.Basic;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context; 
    protected readonly DbSet<T> _dbSet;
    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>(); 
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        //await _context.SaveChangesAsync();
        return entity;
    }
    
    public async Task AddRangeAsync(IEnumerable<T> entities) // Triển khai AddRangeAsync
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _dbSet.FindAsync(id);

        if (item == null)
        {
            return false;
        }
        
        _dbSet.Remove(item);
        //await _context.SaveChangesAsync();
        return true;
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity); // Chỉ đánh dấu đối tượng để xóa, không gọi SaveChangesAsync()
    }
    
    public void RemoveRange(IEnumerable<T> entities) // Triển khai RemoveRange
    {
        _dbSet.RemoveRange(entities);
    }
    
    public async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }
    
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

}