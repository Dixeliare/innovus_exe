using System.Net;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Services.Services;

public class SheetMusicService : ISheetMusicService
{
    // private readonly ISheetMusicRepository _sheetMusicRepository;
    // private readonly ISheetRepository _sheetRepository;
    // private readonly IGenreRepository _genreRepository;
    // private readonly IFileStorageService _fileStorageService; // Inject IFileStorageService
    //
    // public SheetMusicService(ISheetMusicRepository sheetMusicRepository,
    //                          ISheetRepository sheetRepository,
    //                          IGenreRepository genreRepository,
    //                          IFileStorageService fileStorageService) // Thêm IFileStorageService vào constructor
    // {
    //     _sheetMusicRepository = sheetMusicRepository;
    //     _sheetRepository = sheetRepository;
    //     _genreRepository = genreRepository;
    //     _fileStorageService = fileStorageService; // Khởi tạo
    // }

    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public SheetMusicService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<IEnumerable<SheetMusicDto>> GetAllAsync()
    {
        var sheetMusics = await _unitOfWork.SheetMusics.GetAllAsync();
        return sheetMusics.Select(MapToSheetMusicDto);
    }

    public async Task<SheetMusicDto> GetByIdAsync(int id)
    {
        var sheetMusic = await _unitOfWork.SheetMusics.GetByIdAsync(id);
        if (sheetMusic == null)
        {
            throw new NotFoundException("SheetMusic", "Id", id);
        }
        return MapToSheetMusicDto(sheetMusic);
    }

    // Add Sheet Music với file ảnh bìa
    public async Task<SheetMusicDto> AddAsync(IFormFile coverImageFile, int? number, string? musicName, string composer,
        int? sheetQuantity, int? favoriteCount, int? sheetId)
    {
        // Kiểm tra tệp ảnh bìa bắt buộc
        if (coverImageFile == null || coverImageFile.Length == 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "CoverImageFile", new string[] { "Tệp ảnh bìa là bắt buộc." } }
            });
        }

        // Kiểm tra sự tồn tại của khóa ngoại SheetId nếu được cung cấp
        if (sheetId.HasValue)
        {
            var sheetExists = await _unitOfWork.Sheets.GetByIdAsync(sheetId.Value);
            if (sheetExists == null)
            {
                throw new NotFoundException("Sheet", "Id", sheetId.Value);
            }
        }

        string coverUrl = string.Empty;
        try
        {
            // Lưu tệp vào Azure Blob Storage
            coverUrl = await _fileStorageService.SaveFileAsync(coverImageFile, "cover-images");
        }
        catch (Exception ex)
        {
            throw new ApiException("Có lỗi xảy ra khi lưu tệp ảnh bìa.", ex, (int)HttpStatusCode.InternalServerError);
        }

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

        try
        {
            var addedSheetMusic = await _unitOfWork.SheetMusics.AddAsync(sheetMusicEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
            return MapToSheetMusicDto(addedSheetMusic);
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu có lỗi DB, dọn dẹp tệp đã tải lên
            if (!string.IsNullOrEmpty(coverUrl))
            {
                await _fileStorageService.DeleteFileAsync(coverUrl);
            }
            throw new ApiException("Có lỗi xảy ra khi thêm bản nhạc vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            // Dọn dẹp tệp đã tải lên nếu có lỗi không mong muốn
            if (!string.IsNullOrEmpty(coverUrl))
            {
                await _fileStorageService.DeleteFileAsync(coverUrl);
            }
            throw new ApiException("An unexpected error occurred while adding the sheet music.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // UPDATE Sheet Music với file ảnh bìa
    public async Task UpdateAsync(int sheetMusicId, IFormFile? coverImageFile, int? number, string? musicName,
        string? composer, int? sheetQuantity, int? favoriteCount, int? sheetId)
    {
        var existingSheetMusic = await _unitOfWork.SheetMusics.GetByIdAsync(sheetMusicId);

        if (existingSheetMusic == null)
        {
            throw new NotFoundException("SheetMusic", "Id", sheetMusicId);
        }

        string? oldCoverUrl = existingSheetMusic.cover_url;
        string? newCoverUrl = null;

        // Xử lý tệp ảnh bìa mới nếu có
        if (coverImageFile != null && coverImageFile.Length > 0)
        {
            try
            {
                // Lưu ảnh mới
                newCoverUrl = await _fileStorageService.SaveFileAsync(coverImageFile, "cover-images");
                existingSheetMusic.cover_url = newCoverUrl; // Cập nhật URL mới
            }
            catch (Exception ex)
            {
                throw new ApiException("Có lỗi xảy ra khi lưu tệp ảnh bìa mới để cập nhật.", ex, (int)HttpStatusCode.InternalServerError);
            }
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

        // sheet_quantity đã được xử lý ở trên
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
            // Chỉ cập nhật nếu SheetId thực sự đã thay đổi
            if (existingSheetMusic.sheet_id != sheetId.Value)
            {
                var sheetExists = await _unitOfWork.Sheets.GetByIdAsync(sheetId.Value);
                if (sheetExists == null)
                {
                    // Nếu tệp đã được tải lên, thử dọn dẹp trước khi ném NotFoundException
                    if (!string.IsNullOrEmpty(newCoverUrl))
                    {
                        await _fileStorageService.DeleteFileAsync(newCoverUrl);
                    }
                    throw new NotFoundException("Sheet", "Id", sheetId.Value);
                }
                existingSheetMusic.sheet_id = sheetId.Value;
            }
        }
        else if (sheetId == null) // Nếu client gửi sheetId = null để xóa liên kết
        {
            existingSheetMusic.sheet_id = null;
        }

        try
        {
            await _unitOfWork.SheetMusics.UpdateAsync(existingSheetMusic);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB

            // Xóa tệp cũ chỉ sau khi cập nhật DB thành công VÀ có tệp mới được tải lên
            if (!string.IsNullOrEmpty(oldCoverUrl) && !string.IsNullOrEmpty(newCoverUrl) && oldCoverUrl != newCoverUrl)
            {
                await _fileStorageService.DeleteFileAsync(oldCoverUrl);
            }
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu cập nhật DB thất bại, dọn dẹp tệp mới đã tải lên nếu nó tồn tại
            if (!string.IsNullOrEmpty(newCoverUrl))
            {
                await _fileStorageService.DeleteFileAsync(newCoverUrl);
            }
            throw new ApiException("Có lỗi xảy ra khi cập nhật bản nhạc trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            // Dọn dẹp tệp mới đã tải lên nếu nó tồn tại
            if (!string.IsNullOrEmpty(newCoverUrl))
            {
                await _fileStorageService.DeleteFileAsync(newCoverUrl);
            }
            throw new ApiException("An unexpected error occurred while updating the sheet music.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var sheetMusicToDelete = await _unitOfWork.SheetMusics.GetByIdAsync(id);
        if (sheetMusicToDelete == null)
        {
            throw new NotFoundException("SheetMusic", "Id", id);
        }

        // Lưu URL trước khi xóa thực thể khỏi DB
        string? coverUrlToDelete = sheetMusicToDelete.cover_url;

        try
        {
            await _unitOfWork.SheetMusics.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB

            // Chỉ xóa tệp khỏi bộ nhớ sau khi xóa DB thành công
            if (!string.IsNullOrEmpty(coverUrlToDelete))
            {
                await _fileStorageService.DeleteFileAsync(coverUrlToDelete);
            }
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu xóa DB thất bại (ví dụ: ràng buộc khóa ngoại), không xóa tệp.
            throw new ApiException("Không thể xóa bản nhạc do lỗi cơ sở dữ liệu (ví dụ: đang được tham chiếu bởi các bảng khác).", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the sheet music.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task AddGenreToSheetMusicAsync(int sheetMusicId, int genreId)
    {
        var sheetMusic = await _unitOfWork.SheetMusics.GetByIdAsync(sheetMusicId);
        if (sheetMusic == null)
        {
            throw new NotFoundException("SheetMusic", "Id", sheetMusicId);
        }

        var genre = await _unitOfWork.Genres.GetByIdAsync(genreId);
        if (genre == null)
        {
            throw new NotFoundException("Genre", "Id", genreId);
        }

        try
        {
            await _unitOfWork.SheetMusics.AddGenreToSheetMusicAsync(sheetMusicId, genreId);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            // Có thể xảy ra nếu mối quan hệ đã tồn tại hoặc có lỗi DB khác
            throw new ApiException("Có lỗi xảy ra khi thêm thể loại vào bản nhạc (có thể đã tồn tại).", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding genre to sheet music.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task RemoveGenreFromSheetMusicAsync(int sheetMusicId, int genreId)
    {
        var sheetMusic = await _unitOfWork.SheetMusics.GetByIdAsync(sheetMusicId);
        if (sheetMusic == null)
        {
            throw new NotFoundException("SheetMusic", "Id", sheetMusicId);
        }

        var genre = await _unitOfWork.Genres.GetByIdAsync(genreId);
        if (genre == null)
        {
            throw new NotFoundException("Genre", "Id", genreId);
        }

        try
        {
            await _unitOfWork.SheetMusics.RemoveGenreFromSheetMusicAsync(sheetMusicId, genreId);
            await _unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi xóa thể loại khỏi bản nhạc.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while removing genre from sheet music.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<SheetMusicDto>> SearchSheetMusicAsync(int? number = null, string? musicName = null,
        string? composer = null,
        int? sheetQuantity = null, int? favoriteCount = null)
    {
        var sheetMusics = await _unitOfWork.SheetMusics.SearchSheetMusicAsync(number, musicName, composer, sheetQuantity,
            favoriteCount);
        return sheetMusics.Select(MapToSheetMusicDto);
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