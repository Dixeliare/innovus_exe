using Repository.Models;

namespace Services.IServices;

public interface IUserFavoriteSheetService
{
    Task<bool> AddUserFavoriteSheetAsync(int userId, int sheetMusicId, bool isFavorite = true);
    Task<bool> UpdateUserFavoriteSheetAsync(int userId, int sheetMusicId, bool isFavorite);
    Task<bool> DeleteUserFavoriteSheetAsync(int userId, int sheetMusicId);

    Task<user_favorite_sheet?> GetUserFavoriteSheetEntryAsync(int userId, int sheetMusicId);
    Task<IEnumerable<sheet_music>> GetFavoriteSheetsByUserAsync(int userId); // Trả về list SheetMusic
    Task<IEnumerable<user>> GetUsersFavoritingSheetAsync(int sheetMusicId); // Trả về list User
    Task<bool> CheckIfSheetIsFavoriteForUserAsync(int userId, int sheetMusicId);
}