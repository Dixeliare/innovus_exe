using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class SheetMusicService : ISheetMusicService
{
    private readonly SheetMusicRepository _sheetMusicRepository;
    private readonly SheetRepository _sheetRepository; // Để kiểm tra khóa ngoại sheet_id
    private readonly GenreRepository _genreRepository; // Để kiểm tra khóa ngoại genre_id

    public SheetMusicService(SheetMusicRepository sheetMusicRepository,
        SheetRepository sheetRepository,
        GenreRepository genreRepository)
    {
        _sheetMusicRepository = sheetMusicRepository;
        _sheetRepository = sheetRepository;
        _genreRepository = genreRepository;
    }
    
    public async Task<IEnumerable<sheet_music>> GetAllAsync()
    {
        return await _sheetMusicRepository.GetAllAsync();
    }

    public async Task<sheet_music> GetByIdAsync(int id)
    {
        return await _sheetMusicRepository.GetByIdAsync(id);
    }

    public async Task<SheetMusicDto> AddAsync(CreateSheetMusicDto createSheetMusicDto)
        {
            // Kiểm tra sự tồn tại của khóa ngoại SheetId nếu được cung cấp
            if (createSheetMusicDto.SheetId.HasValue)
            {
                var sheetExists = await _sheetRepository.GetByIdAsync(createSheetMusicDto.SheetId.Value);
                if (sheetExists == null)
                {
                    throw new KeyNotFoundException($"Sheet with ID {createSheetMusicDto.SheetId} not found.");
                }
            }

            var sheetMusicEntity = new sheet_music
            {
                number = createSheetMusicDto.Number,
                music_name = createSheetMusicDto.MusicName,
                composer = createSheetMusicDto.Composer,
                cover_url = createSheetMusicDto.CoverUrl,
                sheet_quantity = createSheetMusicDto.SheetQuantity,
                favorite_count = createSheetMusicDto.FavoriteCount ?? 0, // Đảm bảo có giá trị mặc định
                sheet_id = createSheetMusicDto.SheetId
            };

            var addedSheetMusic = await _sheetMusicRepository.AddAsync(sheetMusicEntity);
            return MapToSheetMusicDto(addedSheetMusic);
        }

        // UPDATE Sheet Music
        public async Task UpdateAsync(UpdateSheetMusicDto updateSheetMusicDto)
        {
            var existingSheetMusic = await _sheetMusicRepository.GetByIdAsync(updateSheetMusicDto.SheetMusicId);

            if (existingSheetMusic == null)
            {
                throw new KeyNotFoundException($"Sheet Music with ID {updateSheetMusicDto.SheetMusicId} not found.");
            }

            // Cập nhật các trường nếu có giá trị được cung cấp
            if (updateSheetMusicDto.Number.HasValue)
            {
                existingSheetMusic.number = updateSheetMusicDto.Number.Value;
            }
            if (!string.IsNullOrEmpty(updateSheetMusicDto.MusicName))
            {
                existingSheetMusic.music_name = updateSheetMusicDto.MusicName;
            }
            if (!string.IsNullOrEmpty(updateSheetMusicDto.Composer))
            {
                existingSheetMusic.composer = updateSheetMusicDto.Composer;
            }
            if (!string.IsNullOrEmpty(updateSheetMusicDto.CoverUrl))
            {
                existingSheetMusic.cover_url = updateSheetMusicDto.CoverUrl;
            }
            if (updateSheetMusicDto.SheetQuantity.HasValue)
            {
                existingSheetMusic.sheet_quantity = updateSheetMusicDto.SheetQuantity.Value;
            }
            if (updateSheetMusicDto.FavoriteCount.HasValue)
            {
                existingSheetMusic.favorite_count = updateSheetMusicDto.FavoriteCount.Value;
            }

            // Cập nhật khóa ngoại SheetId nếu có giá trị mới được cung cấp
            if (updateSheetMusicDto.SheetId.HasValue)
            {
                if (existingSheetMusic.sheet_id != updateSheetMusicDto.SheetId.Value) // Chỉ cập nhật nếu thay đổi
                {
                    var sheetExists = await _sheetRepository.GetByIdAsync(updateSheetMusicDto.SheetId.Value);
                    if (sheetExists == null)
                    {
                        throw new KeyNotFoundException($"Sheet with ID {updateSheetMusicDto.SheetId} not found for update.");
                    }
                    existingSheetMusic.sheet_id = updateSheetMusicDto.SheetId.Value;
                }
            }
            else if (updateSheetMusicDto.SheetId == null) // Nếu gán null, xóa liên kết
            {
                existingSheetMusic.sheet_id = null;
            }


            await _sheetMusicRepository.UpdateAsync(existingSheetMusic);
        }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _sheetMusicRepository.DeleteAsync(id);
    }
    
    public async Task AddGenreToSheetMusicAsync(int sheetMusicId, int genreId)
    {
        // Kiểm tra sự tồn tại của Genre
        var genreExists = await _genreRepository.GetByIdAsync(genreId);
        if (genreExists == null)
        {
            throw new KeyNotFoundException($"Genre with ID {genreId} not found.");
        }
        await _sheetMusicRepository.AddGenreToSheetMusicAsync(sheetMusicId, genreId);
    }

    public async Task RemoveGenreFromSheetMusicAsync(int sheetMusicId, int genreId)
    {
        await _sheetMusicRepository.RemoveGenreFromSheetMusicAsync(sheetMusicId, genreId);
    }

    public async Task<IEnumerable<sheet_music>> SearchSheetMusicAsync(int? number = null, string? musicName = null, string? composer = null,
        int? sheetQuantity = null, int? favoriteCount = null)
    {
        return await _sheetMusicRepository.SearchSheetMusicAsync(number, musicName, composer, sheetQuantity, favoriteCount);
    }
    
    private SheetMusicDto MapToSheetMusicDto(sheet_music model)
    {
        return new SheetMusicDto
        {
            SheetMusicId = model.sheet_music_id,
            Number = model.number,
            MusicName = model.music_name,
            Composer = model.composer,
            CoverUrl = model.cover_url,
            SheetQuantity = model.sheet_quantity,
            FavoriteCount = model.favorite_count,
            SheetId = model.sheet_id // Bao gồm khóa ngoại
        };
    }
}