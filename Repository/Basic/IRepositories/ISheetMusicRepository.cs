using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface ISheetMusicRepository
{
    Task<IEnumerable<sheet_music>> GetAllAsync();
    Task<sheet_music> GetByIdAsync(int id);
    Task<sheet_music> AddAsync(sheet_music entity);
    Task UpdateAsync(sheet_music entity);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<sheet_music>> SearchSheetMusicAsync(
        int? number = null,
        string? musicName = null,
        string? composer = null,
        int? sheetQuantity = null,
        int? favoriteCount = null);

    Task AddGenreToSheetMusicAsync(int sheetMusicId, int genreId);
    Task RemoveGenreFromSheetMusicAsync(int sheetMusicId, int genreId);
}