using Repository.Models;

namespace Services.IServices;

public interface ISheetMusicService
{
    Task<IEnumerable<sheet_music>> GetAllAsync();
    Task<sheet_music> GetByIdAsync(int id);
    Task<int> CreateAsync(sheet_music entity);
    Task<int> UpdateAsync(sheet_music entity);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<sheet_music>> SearchSheetMusicAsync(
        int? number = null,
        string? musicName = null,
        string? composer = null,
        int? sheetQuantity = null,
        int? favoriteCount = null);
}