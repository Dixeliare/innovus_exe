using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IUserFavoriteSheetRepository
{
    Task<IEnumerable<user_favorite_sheet>> GetAllAsync();
    Task<user_favorite_sheet?> GetByIdAsync(int userId, int sheetMusicId);
    Task<user_favorite_sheet> AddAsync(user_favorite_sheet entity);
    Task UpdateAsync(user_favorite_sheet entity);
    Task<int> DeleteAsync(int userId, int sheetMusicId);
    Task<user_favorite_sheet?> GetByUserAndSheetMusicIdAsync(int userId, int sheetMusicId);
    Task<IEnumerable<user_favorite_sheet>> GetUserFavoriteSheetsAsync(int userId);
    Task<IEnumerable<user_favorite_sheet>> GetUsersWhoFavoritedSheetAsync(int sheetMusicId);
    Task<bool> IsSheetFavoriteForUserAsync(int userId, int sheetMusicId);
}