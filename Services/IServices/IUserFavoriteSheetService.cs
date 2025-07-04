using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IUserFavoriteSheetService
{
    Task<UserFavoriteSheetDto> AddUserFavoriteSheetAsync(CreateUserFavoriteSheetDto createDto);
    Task UpdateUserFavoriteSheetAsync(UpdateUserFavoriteSheetDto updateDto);
    Task DeleteUserFavoriteSheetAsync(int userId, int sheetMusicId);

    Task<user_favorite_sheet> GetUserFavoriteSheetEntryAsync(int userId, int sheetMusicId);
    Task<IEnumerable<sheet_music>> GetFavoriteSheetsByUserAsync(int userId); // Trả về list SheetMusic
    Task<IEnumerable<user>> GetUsersFavoritingSheetAsync(int sheetMusicId); // Trả về list User
    Task<bool> CheckIfSheetIsFavoriteForUserAsync(int userId, int sheetMusicId);
    Task<UserFavoriteSheetDto> GetByIdAsync(int userId, int sheetMusicId);
}