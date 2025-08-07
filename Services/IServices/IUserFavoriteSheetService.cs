using DTOs;

namespace Services.IServices;

public interface IUserFavoriteSheetService
{
    Task<IEnumerable<UserFavoriteSheetDto>> GetAllAsync();
    Task<UserFavoriteSheetDto> GetByIdAsync(int userId, int sheetMusicId);
    Task<UserFavoriteSheetListDto> GetUserFavoritesAsync(int userId);
    Task<UserFavoriteSheetDto> AddAsync(CreateUserFavoriteSheetDto createDto);
    Task UpdateAsync(UpdateUserFavoriteSheetDto updateDto);
    Task DeleteAsync(int userId, int sheetMusicId);
    Task ToggleFavoriteAsync(int userId, int sheetMusicId);
    Task<bool> IsFavoriteAsync(int userId, int sheetMusicId);
}