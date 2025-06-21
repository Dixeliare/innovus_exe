using DTOs;
using Microsoft.AspNetCore.Http;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class SheetMusicService : ISheetMusicService
{
    private readonly ISheetMusicRepository _sheetMusicRepository;
    private readonly ISheetRepository _sheetRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IFileStorageService _fileStorageService; // Inject IFileStorageService

    public SheetMusicService(ISheetMusicRepository sheetMusicRepository,
                             ISheetRepository sheetRepository,
                             IGenreRepository genreRepository,
                             IFileStorageService fileStorageService) // Thêm IFileStorageService vào constructor
    {
        _sheetMusicRepository = sheetMusicRepository;
        _sheetRepository = sheetRepository;
        _genreRepository = genreRepository;
        _fileStorageService = fileStorageService; // Khởi tạo
    }

    public async Task<IEnumerable<sheet_music>> GetAllAsync()
    {
        return await _sheetMusicRepository.GetAllAsync();
    }

    public async Task<sheet_music> GetByIdAsync(int id)
    {
        return await _sheetMusicRepository.GetByIdAsync(id);
    }

    // Add Sheet Music với file ảnh bìa
    public async Task<SheetMusicDto> AddAsync(IFormFile coverImageFile, int? number, string? musicName, string composer, int? sheetQuantity, int? favoriteCount, int? sheetId)
    {
        // Kiểm tra sự tồn tại của khóa ngoại SheetId nếu được cung cấp
        if (sheetId.HasValue)
        {
            var sheetExists = await _sheetRepository.GetByIdAsync(sheetId.Value);
            if (sheetExists == null)
            {
                throw new KeyNotFoundException($"Sheet with ID {sheetId.Value} not found.");
            }
        }

        string coverUrl = string.Empty;
        if (coverImageFile == null || coverImageFile.Length == 0)
        {
            // Vì cover_url là null! trong model, nó không thể là null trong DB.
            // Nếu bạn không muốn người dùng upload ảnh, bạn phải có một URL mặc định.
            // Ở đây, tôi sẽ ném lỗi nếu không có file, bạn có thể thay đổi để gán URL mặc định.
            throw new ArgumentException("Cover image file is required.");
        }

        // Lưu file vào Azure Blob Storage
        // Sử dụng "cover-images" làm folder logic trong container
        coverUrl = await _fileStorageService.SaveFileAsync(coverImageFile, "cover-images");

        var sheetMusicEntity = new sheet_music
        {
            number = number,
            music_name = musicName,
            composer = composer,
            cover_url = coverUrl, // Gán URL từ Azure Blob
            sheet_quantity = sheetQuantity,
            favorite_count = favoriteCount ?? 0,
            sheet_id = sheetId
        };

        var addedSheetMusic = await _sheetMusicRepository.AddAsync(sheetMusicEntity);
        return MapToSheetMusicDto(addedSheetMusic);
    }

    // UPDATE Sheet Music với file ảnh bìa
    public async Task UpdateAsync(int sheetMusicId, IFormFile? coverImageFile, int? number, string? musicName, string? composer, int? sheetQuantity, int? favoriteCount, int? sheetId)
    {
        var existingSheetMusic = await _sheetMusicRepository.GetByIdAsync(sheetMusicId);

        if (existingSheetMusic == null)
        {
            throw new KeyNotFoundException($"Sheet Music with ID {sheetMusicId} not found.");
        }

        // Xử lý file ảnh mới nếu có
        if (coverImageFile != null && coverImageFile.Length > 0)
        {
            // 1. Xóa ảnh cũ (nếu có và không rỗng)
            if (!string.IsNullOrEmpty(existingSheetMusic.cover_url))
            {
                await _fileStorageService.DeleteFileAsync(existingSheetMusic.cover_url);
            }

            // 2. Lưu ảnh mới
            string newCoverUrl = await _fileStorageService.SaveFileAsync(coverImageFile, "cover-images");
            existingSheetMusic.cover_url = newCoverUrl; // Cập nhật URL mới
        }
        // Nếu coverImageFile là null, giữ nguyên cover_url hiện có.
        // Điều này hợp lý vì cover_url là NOT NULL trong DB.

        // Cập nhật các trường khác nếu có giá trị được cung cấp
        if (number.HasValue)
        {
            existingSheetMusic.number = number.Value;
        }
        if (!string.IsNullOrEmpty(musicName))
        {
            existingSheetMusic.music_name = musicName;
        }
        if (!string.IsNullOrEmpty(composer))
        {
            existingSheetMusic.composer = composer;
        }
        // cover_url đã được xử lý ở trên
        if (sheetQuantity.HasValue)
        {
            existingSheetMusic.sheet_quantity = sheetQuantity.Value;
        }
        if (favoriteCount.HasValue)
        {
            existingSheetMusic.favorite_count = favoriteCount.Value;
        }

        // Cập nhật khóa ngoại SheetId nếu có giá trị mới được cung cấp
        if (sheetId.HasValue)
        {
            if (existingSheetMusic.sheet_id != sheetId.Value)
            {
                var sheetExists = await _sheetRepository.GetByIdAsync(sheetId.Value);
                if (sheetExists == null)
                {
                    throw new KeyNotFoundException($"Sheet with ID {sheetId.Value} not found for update.");
                }
                existingSheetMusic.sheet_id = sheetId.Value;
            }
        }
        else if (sheetId == null)
        {
            existingSheetMusic.sheet_id = null;
        }

        await _sheetMusicRepository.UpdateAsync(existingSheetMusic);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var sheetMusicToDelete = await _sheetMusicRepository.GetByIdAsync(id);
        if (sheetMusicToDelete == null)
        {
            return false;
        }

        // Xóa file ảnh bìa liên quan khỏi Azure Blob trước
        if (!string.IsNullOrEmpty(sheetMusicToDelete.cover_url))
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(sheetMusicToDelete.cover_url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting blob for sheet music {id}: {ex.Message}");
            }
        }

        return await _sheetMusicRepository.DeleteAsync(id);
    }

    public async Task AddGenreToSheetMusicAsync(int sheetMusicId, int genreId)
    {
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
            SheetId = model.sheet_id
        };
    }
}