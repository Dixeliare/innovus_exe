using DTOs;
using Microsoft.AspNetCore.Http;
using Repository.Models;

namespace Services.IServices;

public interface ISheetMusicService
{
    Task<IEnumerable<SheetMusicDto>> GetAllAsync();
    Task<SheetMusicDto> GetByIdAsync(int id);
    // Thay đổi AddAsync để nhận IFormFile và các thuộc tính khác
    Task<SheetMusicDto> AddAsync(IFormFile coverImageFile, int? number, string? musicName, string composer, int? sheetQuantity, int? favoriteCount, int? sheetId);

    // Thay đổi UpdateAsync để nhận IFormFile và các thuộc tính khác
    Task UpdateAsync(int sheetMusicId, IFormFile? coverImageFile, int? number, string? musicName, string? composer, int? sheetQuantity, int? favoriteCount, int? sheetId);

    Task DeleteAsync(int id);
    Task AddGenreToSheetMusicAsync(int sheetMusicId, int genreId);
    Task RemoveGenreFromSheetMusicAsync(int sheetMusicId, int genreId);

    Task<IEnumerable<SheetMusicDto>> SearchSheetMusicAsync(
        int? number = null,
        string? musicName = null,
        string? composer = null,
        int? sheetQuantity = null,
        int? favoriteCount = null);
}