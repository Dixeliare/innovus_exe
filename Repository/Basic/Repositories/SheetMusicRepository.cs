using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class SheetMusicRepository : GenericRepository<sheet_music>
{
    public SheetMusicRepository()
    {
    }
    
    public SheetMusicRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<sheet_music>> GetAllAsync()
    {
        return await _context.sheet_musics
            .Include(s => s.sheet)
            .Include(u => u.user_favorite_sheets)
            .Include(g => g.genres)
            .ToListAsync();
    }

    public async Task<sheet_music> GetByIdAsync(int id)
    {
        return await _context.sheet_musics
            .Include(s => s.sheet)
            .Include(u => u.user_favorite_sheets)
            .Include(g => g.genres)
            .FirstOrDefaultAsync(s => s.sheet_music_id == id);
    }

    public async Task<int> CreateAsync(sheet_music entity)
    {
        await _context.sheet_musics.AddAsync(entity);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(sheet_music entity)
    {
        var item = await _context.sheet_musics.FindAsync(entity.sheet_music_id);
        
        if (item == null) return 0;
        
        item.number = entity.number;
        item.music_name = entity.music_name;
        item.composer = entity.composer;
        item.cover_url = entity.cover_url;
        item.sheet_quantity = entity.sheet_quantity;
        item.favorite_count = entity.favorite_count;
        item.sheet_id = entity.sheet_id;
        
        return await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.sheet_musics.FindAsync(id);
        
        if (item == null) return false;
        
        _context.sheet_musics.Remove(item); 
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<IEnumerable<sheet_music>> SearchSheetMusicAsync(
            int? number = null,
            string? musicName = null,
            string? composer = null,
            int? sheetQuantity = null,
            int? favoriteCount = null)
        {
            IQueryable<sheet_music> query = _context.sheet_musics;

            // Luôn bao gồm các navigation property bạn muốn trả về cùng kết quả
            query = query.Include(sm => sm.sheet)
                         .Include(sm => sm.genres);

            // Xây dựng danh sách các biểu thức điều kiện (predicates)
            var predicates = new List<Expression<Func<sheet_music, bool>>>();

            if (number.HasValue)
            {
                predicates.Add(sm => sm.number == number.Value);
            }

            if (!string.IsNullOrEmpty(musicName))
            {
                // Tìm kiếm không phân biệt chữ hoa/thường và khớp một phần
                var lowerMusicName = musicName.ToLower();
                predicates.Add(sm => sm.music_name != null && sm.music_name.ToLower().Contains(lowerMusicName));
            }

            if (!string.IsNullOrEmpty(composer))
            {
                var lowerComposer = composer.ToLower();
                predicates.Add(sm => sm.composer != null && sm.composer.ToLower().Contains(lowerComposer));
            }

            if (sheetQuantity.HasValue)
            {
                predicates.Add(sm => sm.sheet_quantity == sheetQuantity.Value);
            }

            if (favoriteCount.HasValue)
            {
                predicates.Add(sm => sm.favorite_count == favoriteCount.Value);
            }

            // Nếu có ít nhất một điều kiện tìm kiếm được cung cấp
            if (predicates.Any())
            {
                // Bắt đầu với biểu thức đầu tiên
                Expression<Func<sheet_music, bool>> combinedPredicate = predicates.First();

                // Nối các biểu thức còn lại bằng toán tử OR
                for (int i = 1; i < predicates.Count; i++)
                {
                    combinedPredicate = Expression.Lambda<Func<sheet_music, bool>>(
                        Expression.OrElse(combinedPredicate.Body, predicates[i].Body),
                        combinedPredicate.Parameters);
                }

                // Áp dụng biểu thức tổng hợp vào truy vấn
                query = query.Where(combinedPredicate);
            }
            // Nếu predicates rỗng (không có tiêu chí nào được cung cấp), query sẽ trả về tất cả.

            return await query.ToListAsync();
        }
}