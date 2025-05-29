using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class GenreRepository : GenericRepository<genre>
{
    public GenreRepository()
    {
    }
    
    public GenreRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<genre>> GetAllAsync()
    {
        return await _context.genres
            .Include(s => s.sheet_musics)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<genre> GetByIdAsync(int id)
    {
        return await _context.genres
            .Include(s => s.sheet_musics)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.genre_id == id);
    }

    public async Task<int> CreateAsync(genre genre)
    {
        await _context.genres.AddAsync(genre);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(genre genre)
    {
        var item = await _context.genres.FindAsync(genre.genre_id);

        if (item == null)
        {
            return 0;
        }
        
        item.genre_name = genre.genre_name;
        
        return await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.genres.FindAsync(id);

        if (item == null)
        {
            return false;
        }
        
        _context.genres.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<IEnumerable<genre>> SearchGenresAsync(string? genreName = null)
    {
        IQueryable<genre> query = _context.genres;

        // Áp dụng điều kiện tìm kiếm nếu genreName được cung cấp
        if (!string.IsNullOrEmpty(genreName))
        {
            // Sử dụng EF.Functions.ILike cho tìm kiếm không phân biệt chữ hoa/thường và khớp một phần
            query = query.Where(g => EF.Functions.ILike(g.genre_name, $"%{genreName}%"));
        }
        // Nếu genreName là null hoặc rỗng, query sẽ không bị lọc và trả về tất cả.

        // Bạn có thể thêm .Include() nếu muốn eager load các navigation properties
        // Ví dụ: .Include(g => g.sheet_musics)

        return await query.ToListAsync();
    }
}