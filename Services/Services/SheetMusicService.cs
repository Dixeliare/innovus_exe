using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class SheetMusicService : ISheetMusicService
{
    private readonly SheetMusicRepository _sheetMusicRepository;
    
    public SheetMusicService(SheetMusicRepository sheetMusicRepository) => _sheetMusicRepository = sheetMusicRepository;
    
    public async Task<IEnumerable<sheet_music>> GetAllAsync()
    {
        return await _sheetMusicRepository.GetAllAsync();
    }

    public async Task<sheet_music> GetByIdAsync(int id)
    {
        return await _sheetMusicRepository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(sheet_music entity)
    {
        return await _sheetMusicRepository.CreateAsync(entity);
    }

    public async Task<int> UpdateAsync(sheet_music entity)
    {
        return await _sheetMusicRepository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _sheetMusicRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<sheet_music>> SearchSheetMusicAsync(int? number = null, string? musicName = null, string? composer = null,
        int? sheetQuantity = null, int? favoriteCount = null)
    {
        return await _sheetMusicRepository.SearchSheetMusicAsync(number, musicName, composer, sheetQuantity, favoriteCount);
    }
}