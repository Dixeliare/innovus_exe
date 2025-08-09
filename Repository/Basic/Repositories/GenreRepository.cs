using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class GenreRepository : GenericRepository<genre>, IGenreRepository
{
    public GenreRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<genre>> GetAllAsync()
    {
        return await _dbSet
            .Include(s => s.sheet_musics)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<genre> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(s => s.sheet_musics)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.genre_id == id);
    }

    // public async Task<genre> AddAsync(genre entity)
    // {
    //     _context.genres.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(genre entity)
    // {
    //     _context.genres.Update(entity);
    //     await _context.SaveChangesAsync();
    // }
    //
    // public async Task<bool> DeleteAsync(int id)
    // {
    //     var item = await _context.genres.FindAsync(id);
    //
    //     if (item == null)
    //     {
    //         return false;
    //     }
    //     
    //     _context.genres.Remove(item);
    //     return await _context.SaveChangesAsync() > 0;
    // }
    
    public async Task<IEnumerable<genre>> SearchGenresAsync(string? genreName = null)
    {
        IQueryable<genre> query = _dbSet;

        // Áp dụng điều kiện tìm kiếm nếu genreName được cung cấp
        if (!string.IsNullOrEmpty(genreName))
        {
            // Sử dụng EF.Functions.ILike cho tìm kiếm không phân biệt chữ hoa/thường và khớp một phần
            query = query.Where(g => EF.Functions.ILike(g.genre_name, $"%{genreName}%"));
        }
        // Nếu genreName là null hoặc rỗng, query sẽ không bị lọc và trả về tất cả.

        // Include sheet_musics để load các navigation properties
        query = query.Include(g => g.sheet_musics)
                    .AsSplitQuery();

        return await query.ToListAsync();
    }
}