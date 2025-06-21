using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class UserFavoriteSheetRepository : GenericRepository<user_favorite_sheet>, IUserFavoriteSheetRepository
{
    public UserFavoriteSheetRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<user_favorite_sheet>> GetAllAsync()
    {
        return await _dbSet
            .Include(s => s.sheet_music)
            .Include(u => u.user)
            .ToListAsync();
    }
    
    public async Task<user_favorite_sheet?> GetByIdAsync(int userId, int sheetMusicId)
    {
        return await _dbSet
            .Include(ufs => ufs.user)
            .Include(ufs => ufs.sheet_music)
            .AsNoTracking()
            .FirstOrDefaultAsync(ufs => ufs.user_id == userId && ufs.sheet_music_id == sheetMusicId);
    }

    // public async Task<user_favorite_sheet> AddAsync(user_favorite_sheet entity)
    // {
    //     _context.user_favorite_sheets.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(user_favorite_sheet entity)
    // {
    //     _context.Entry(entity).State = EntityState.Modified;
    //     await _context.SaveChangesAsync();
    // }
    //
    public async Task<int> Delete2ArgumentsAsync(int userId, int sheetMusicId)
    {
        var item = await _context.user_favorite_sheets
            .FirstOrDefaultAsync(ufs => ufs.user_id == userId &&
                                        ufs.sheet_music_id == sheetMusicId);
        if (item == null)
        {
            return 0;
        }
    
        _context.user_favorite_sheets.Remove(item);
        return await _context.SaveChangesAsync();
    }

    // 4 hàm truy vấn chính

    // 1. Lấy một bản ghi cụ thể bằng userId và sheetMusicId
    public async Task<user_favorite_sheet?> GetByUserAndSheetMusicIdAsync(int userId, int sheetMusicId)
    {
        return await _dbSet
            .Include(ufs => ufs.user) // Bao gồm thông tin User
            .Include(ufs => ufs.sheet_music)
            .AsSplitQuery() // Bao gồm thông tin SheetMusic
            .FirstOrDefaultAsync(ufs => ufs.user_id == userId &&
                                        ufs.sheet_music_id == sheetMusicId);
    }

    // 2. Lấy tất cả các bản nhạc yêu thích của một người dùng
    public async Task<IEnumerable<user_favorite_sheet>> GetUserFavoriteSheetsAsync(int userId)
    {
        return await _dbSet
            .Include(ufs => ufs.sheet_music) // Bao gồm thông tin SheetMusic
            .ThenInclude(sm => sm.genres) // Có thể bao gồm cả thể loại của bản nhạc
            .Include(ufs => ufs.user)
            .AsSplitQuery()
            .Where(ufs => ufs.user_id == userId && ufs.is_favorite == true) // Chỉ lấy các mục thực sự là yêu thích
            .ToListAsync();
    }

    // 3. Lấy tất cả người dùng đã yêu thích một bản nhạc cụ thể
    public async Task<IEnumerable<user_favorite_sheet>> GetUsersWhoFavoritedSheetAsync(int sheetMusicId)
    {
        return await _dbSet
            .Include(ufs => ufs.user) // Bao gồm thông tin User
            .Include(ufs => ufs.sheet_music) // Bao gồm thông tin SheetMusic
            .AsSplitQuery()
            .Where(ufs =>
                ufs.sheet_music_id == sheetMusicId && ufs.is_favorite == true) // Chỉ lấy các mục thực sự là yêu thích
            .ToListAsync();
    }

    // 4. Kiểm tra xem một bản nhạc có phải là yêu thích của người dùng không
    public async Task<bool> IsSheetFavoriteForUserAsync(int userId, int sheetMusicId)
    {
        return await _dbSet
            .AnyAsync(ufs => ufs.user_id == userId &&
                             ufs.sheet_music_id == sheetMusicId &&
                             ufs.is_favorite == true);
    }
}