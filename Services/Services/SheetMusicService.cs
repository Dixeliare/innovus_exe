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
        int? sheetQuantity, int? favoriteCount, List<int>? genreIds = null)
    {
        // Kiểm tra tệp ảnh bìa bắt buộc
        if (coverImageFile == null || coverImageFile.Length == 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "CoverImageFile", new string[] { "Tệp ảnh bìa là bắt buộc." } }
            });
        }

        // Không cần kiểm tra sheet_id nữa vì quan hệ đã thay đổi thành 1-n

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
            favorite_count = favoriteCount ?? 0
            // Không cần sheet_id nữa vì quan hệ đã thay đổi thành 1-n
        };

        try
        {
            var addedSheetMusic = await _unitOfWork.SheetMusics.AddAsync(sheetMusicEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
            
            // Assign genres nếu có
            if (genreIds != null && genreIds.Any())
            {
                foreach (var genreId in genreIds)
                {
                    await _unitOfWork.SheetMusics.AddGenreToSheetMusicAsync(addedSheetMusic.sheet_music_id, genreId);
                }
                await _unitOfWork.CompleteAsync();
            }
            
            // Query lại để có đầy đủ navigation properties (genres, sheets)
            var sheetMusicWithRelations = await _unitOfWork.SheetMusics.GetByIdAsync(addedSheetMusic.sheet_music_id);
            return MapToSheetMusicDto(sheetMusicWithRelations);
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
        string? composer, int? sheetQuantity, int? favoriteCount, List<int>? genreIds = null)
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

        // Không cần xử lý sheet_id nữa vì quan hệ đã thay đổi thành 1-n
        // Sheet sẽ được liên kết thông qua sheet_music_id trong bảng sheet

        try
        {
            await _unitOfWork.SheetMusics.UpdateAsync(existingSheetMusic);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB

            // Cập nhật genres nếu có
            if (genreIds != null)
            {
                // Xóa tất cả genres hiện tại
                var currentGenres = existingSheetMusic.genres?.ToList() ?? new List<genre>();
                foreach (var currentGenre in currentGenres)
                {
                    await _unitOfWork.SheetMusics.RemoveGenreFromSheetMusicAsync(sheetMusicId, currentGenre.genre_id);
                }
                
                // Thêm genres mới
                foreach (var genreId in genreIds)
                {
                    await _unitOfWork.SheetMusics.AddGenreToSheetMusicAsync(sheetMusicId, genreId);
                }
                await _unitOfWork.CompleteAsync();
            }

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
            // Quan hệ 1-n: map các sheet liên quan
            Sheets = model.sheets?.Select(s => new SheetDto
            {
                SheetId = s.sheet_id,
                SheetUrl = s.sheet_url,
                SheetMusicId = s.sheet_music_id ?? 0
            }).ToList() ?? new List<SheetDto>(),
            // Quan hệ many-to-many: map các genre
            Genres = model.genres?.Select(g => new GenreBasicDto
            {
                GenreId = g.genre_id,
                GenreName = g.genre_name
            }).ToList() ?? new List<GenreBasicDto>()
        };
    }
}