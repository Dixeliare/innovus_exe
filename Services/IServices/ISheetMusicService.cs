using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface ISheetMusicService
{
    Task<IEnumerable<sheet_music>> GetAllAsync();
    Task<sheet_music> GetByIdAsync(int id);
    Task<SheetMusicDto> AddAsync(CreateSheetMusicDto createSheetMusicDto);
    Task UpdateAsync(UpdateSheetMusicDto updateSheetMusicDto);
    Task<bool> DeleteAsync(int id);
    Task AddGenreToSheetMusicAsync(int sheetMusicId, int genreId);
    Task RemoveGenreFromSheetMusicAsync(int sheetMusicId, int genreId);

    Task<IEnumerable<sheet_music>> SearchSheetMusicAsync(
        int? number = null,
        string? musicName = null,
        string? composer = null,
        int? sheetQuantity = null,
        int? favoriteCount = null);
}