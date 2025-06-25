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

public class SheetService : ISheetService
{
    // private readonly ISheetRepository _sheetRepository;
    // private readonly ISheetMusicRepository _sheetMusicRepository;
    // private readonly IFileStorageService _fileStorageService; // Inject IFileStorageService
    //
    // public SheetService(ISheetRepository sheetRepository,
    //                     ISheetMusicRepository sheetMusicRepository,
    //                     IFileStorageService fileStorageService) // Thêm IFileStorageService vào constructor
    // {
    //     _sheetRepository = sheetRepository;
    //     _sheetMusicRepository = sheetMusicRepository;
    //     _fileStorageService = fileStorageService; // Khởi tạo
    // }
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public SheetService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<IEnumerable<SheetDto>> GetAllAsync()
    {
        var sheets = await _unitOfWork.Sheets.GetAllAsync();
        // Cân nhắc bao gồm sheet_music nếu cần cho việc ánh xạ
        // ví dụ: var sheets = await _unitOfWork.Sheets.GetAllAsync(includeProperties: "sheet_music");
        return sheets.Select(MapToSheetDto);
    }

    public async Task<SheetDto> GetByIdAsync(int id)
    {
        // Cân nhắc bao gồm sheet_music nếu MapToSheetDto cần nó.
        // ví dụ: var sheet = await _unitOfWork.Sheets.GetByIdAsync(id, includeProperties: "sheet_music");
        var sheet = await _unitOfWork.Sheets.GetByIdAsync(id);
        if (sheet == null)
        {
            throw new NotFoundException("Sheet", "Id", id);
        }
        return MapToSheetDto(sheet);
    }

    // Add Sheet với file ảnh
    public async Task<SheetDto> AddAsync(IFormFile sheetFile, int sheetMusicId)
    {
        // Kiểm tra tệp bắt buộc
        if (sheetFile == null || sheetFile.Length == 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "SheetFile", new string[] { "Tệp tin bản nhạc là bắt buộc." } }
            });
        }

        // Kiểm tra sự tồn tại của khóa ngoại SheetMusic
        var sheetMusic = await _unitOfWork.SheetMusics.GetByIdAsync(sheetMusicId);
        if (sheetMusic == null)
        {
            throw new NotFoundException("SheetMusic", "Id", sheetMusicId);
        }

        string sheetUrl = string.Empty;
        try
        {
            // Lưu tệp vào Azure Blob Storage
            sheetUrl = await _fileStorageService.SaveFileAsync(sheetFile, "sheets");
        }
        catch (Exception ex)
        {
            throw new ApiException("Có lỗi xảy ra khi lưu tệp bản nhạc.", ex, (int)HttpStatusCode.InternalServerError);
        }

        var sheetEntity = new sheet
        {
            sheet_url = sheetUrl, // Gán URL từ Azure
            sheet_music = sheetMusic // Gán thực thể thực cho thuộc tính điều hướng
        };

        try
        {
            var addedSheet = await _unitOfWork.Sheets.AddAsync(sheetEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB
            return MapToSheetDto(addedSheet);
        }
        catch (DbUpdateException dbEx) // Bắt lỗi từ Entity Framework
        {
            // Nếu có lỗi ràng buộc duy nhất hoặc lỗi DB khác
            throw new ApiException("Có lỗi xảy ra khi thêm bản nhạc vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            // Dọn dẹp tệp đã tải lên nếu thao tác DB thất bại
            if (!string.IsNullOrEmpty(sheetUrl))
            {
                await _fileStorageService.DeleteFileAsync(sheetUrl);
            }
            throw new ApiException("An unexpected error occurred while adding the sheet.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // UPDATE Sheet với file ảnh
    public async Task UpdateAsync(int sheetId, IFormFile? sheetFile, int? sheetMusicId)
    {
        var existingSheet = await _unitOfWork.Sheets.GetByIdAsync(sheetId);

        if (existingSheet == null)
        {
            throw new NotFoundException("Sheet", "Id", sheetId);
        }

        string? oldSheetUrl = existingSheet.sheet_url;
        string? newSheetUrl = null;

        // Xử lý tệp hình ảnh mới nếu được cung cấp
        if (sheetFile != null && sheetFile.Length > 0)
        {
            try
            {
                // Lưu tệp mới
                newSheetUrl = await _fileStorageService.SaveFileAsync(sheetFile, "sheets");
                existingSheet.sheet_url = newSheetUrl; // Cập nhật với URL mới
            }
            catch (Exception ex)
            {
                throw new ApiException("Có lỗi xảy ra khi lưu tệp bản nhạc mới để cập nhật.", ex, (int)HttpStatusCode.InternalServerError);
            }
        }
        // Nếu sheetFile là null, sheet_url hiện có vẫn giữ nguyên.
        // Nếu bạn muốn cho phép xóa ảnh bằng cách gửi tệp null, hãy thêm logic cụ thể ở đây.

        // Kiểm tra và cập nhật khóa ngoại Sheet Music nếu có giá trị mới được cung cấp
        if (sheetMusicId.HasValue)
        {
            // Chỉ cập nhật nếu SheetMusicId thực sự đã thay đổi để tránh các cuộc gọi DB không cần thiết
            if (existingSheet.sheet_music?.sheet_music_id != sheetMusicId.Value)
            {
                var sheetMusic = await _unitOfWork.SheetMusics.GetByIdAsync(sheetMusicId.Value);
                if (sheetMusic == null)
                {
                    // Nếu tệp đã được tải lên, hãy thử dọn dẹp nó trước khi ném NotFoundException
                    if (!string.IsNullOrEmpty(newSheetUrl))
                    {
                        await _fileStorageService.DeleteFileAsync(newSheetUrl);
                    }
                    throw new NotFoundException("SheetMusic", "Id", sheetMusicId.Value);
                }
                existingSheet.sheet_music = sheetMusic;
            }
        }

        try
        {
            await _unitOfWork.Sheets.UpdateAsync(existingSheet);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB

            // Xóa tệp cũ chỉ sau khi cập nhật DB thành công
            if (!string.IsNullOrEmpty(oldSheetUrl) && !string.IsNullOrEmpty(newSheetUrl))
            {
                await _fileStorageService.DeleteFileAsync(oldSheetUrl);
            }
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu cập nhật DB thất bại, dọn dẹp tệp mới đã tải lên nếu nó tồn tại
            if (!string.IsNullOrEmpty(newSheetUrl))
            {
                await _fileStorageService.DeleteFileAsync(newSheetUrl);
            }
            throw new ApiException("Có lỗi xảy ra khi cập nhật bản nhạc trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            // Dọn dẹp tệp mới đã tải lên nếu nó tồn tại
            if (!string.IsNullOrEmpty(newSheetUrl))
            {
                await _fileStorageService.DeleteFileAsync(newSheetUrl);
            }
            throw new ApiException("An unexpected error occurred while updating the sheet.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // Delete Sheet
    public async Task DeleteAsync(int id)
    {
        var sheetToDelete = await _unitOfWork.Sheets.GetByIdAsync(id);
        if (sheetToDelete == null)
        {
            throw new NotFoundException("Sheet", "Id", id);
        }

        // Lưu URL trước khi xóa thực thể khỏi DB
        string? sheetUrlToDelete = sheetToDelete.sheet_url;

        try
        {
            await _unitOfWork.Sheets.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi vào DB

            // Chỉ xóa tệp khỏi bộ nhớ sau khi xóa DB thành công
            if (!string.IsNullOrEmpty(sheetUrlToDelete))
            {
                await _fileStorageService.DeleteFileAsync(sheetUrlToDelete);
            }
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu xóa DB thất bại (ví dụ: ràng buộc khóa ngoại), không xóa tệp.
            throw new ApiException("Không thể xóa bản nhạc do lỗi cơ sở dữ liệu (ví dụ: đang được sử dụng).", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the sheet.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    private SheetDto MapToSheetDto(sheet model)
    {
        return new SheetDto
        {
            SheetId = model.sheet_id,
            SheetUrl = model.sheet_url,
            SheetMusicId = model.sheet_music?.sheet_music_id ?? 0
        };
    }
}