using DTOs;
using Microsoft.AspNetCore.Http;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class SheetService : ISheetService
{
    private readonly SheetRepository _sheetRepository;
    private readonly SheetMusicRepository _sheetMusicRepository;
    private readonly IFileStorageService _fileStorageService; // Inject IFileStorageService

    public SheetService(SheetRepository sheetRepository,
                        SheetMusicRepository sheetMusicRepository,
                        IFileStorageService fileStorageService) // Thêm IFileStorageService vào constructor
    {
        _sheetRepository = sheetRepository;
        _sheetMusicRepository = sheetMusicRepository;
        _fileStorageService = fileStorageService; // Khởi tạo
    }

    public async Task<IEnumerable<sheet>> GetAllAsync()
    {
        return await _sheetRepository.GetAllAsync();
    }

    public async Task<sheet> GetByIdAsync(int id)
    {
        return await _sheetRepository.GetByIdAsync(id);
    }

    // Add Sheet với file ảnh
    public async Task<SheetDto> AddAsync(IFormFile sheetFile, int sheetMusicId)
    {
        // Kiểm tra sự tồn tại của khóa ngoại SheetMusic
        var sheetMusicExists = await _sheetMusicRepository.GetByIdAsync(sheetMusicId);
        if (sheetMusicExists == null)
        {
            throw new KeyNotFoundException($"Sheet Music with ID {sheetMusicId} not found.");
        }

        string sheetUrl = string.Empty;
        if (sheetFile != null && sheetFile.Length > 0)
        {
            // Lưu file vào Azure Blob Storage
            // Sử dụng "sheets" làm folder logic trong container
            sheetUrl = await _fileStorageService.SaveFileAsync(sheetFile, "sheets");
        }
        else
        {
            // Nếu không có file, có thể gán URL mặc định hoặc để trống tùy ý
            // Trong trường hợp này, nếu sheet_url là null! trong model, thì cần một giá trị.
            // Nếu bạn muốn nó có thể null trong DB, hãy chỉnh sửa model (nhưng bạn không muốn)
            // nên tốt nhất là luôn có file hoặc URL mặc định.
            // Ở đây tôi mặc định là throw lỗi nếu không có file, bạn có thể chỉnh sửa
            throw new ArgumentException("Sheet file is required.");
        }

        var sheetEntity = new sheet
        {
            sheet_url = sheetUrl, // Gán URL từ Azure
            sheet_music = sheetMusicExists
        };

        var addedSheet = await _sheetRepository.AddAsync(sheetEntity);
        return MapToSheetDto(addedSheet);
    }

    // UPDATE Sheet với file ảnh
    public async Task UpdateAsync(int sheetId, IFormFile? sheetFile, int? sheetMusicId)
    {
        var existingSheet = await _sheetRepository.GetByIdAsync(sheetId);

        if (existingSheet == null)
        {
            throw new KeyNotFoundException($"Sheet with ID {sheetId} not found.");
        }

        // Xử lý file ảnh mới nếu có
        if (sheetFile != null && sheetFile.Length > 0)
        {
            // 1. Xóa ảnh cũ (nếu có)
            if (!string.IsNullOrEmpty(existingSheet.sheet_url))
            {
                await _fileStorageService.DeleteFileAsync(existingSheet.sheet_url);
            }

            // 2. Lưu ảnh mới
            string newSheetUrl = await _fileStorageService.SaveFileAsync(sheetFile, "sheets");
            existingSheet.sheet_url = newSheetUrl; // Cập nhật URL mới
        }
        // Nếu sheetFile là null, giữ nguyên sheet_url hiện có.
        // Nếu bạn muốn cho phép xóa ảnh bằng cách gửi null file, bạn cần thêm logic.

        // Kiểm tra và cập nhật khóa ngoại Sheet Music nếu có giá trị mới được cung cấp
        if (sheetMusicId.HasValue)
        {
            if (existingSheet.sheet_music?.sheet_music_id != sheetMusicId.Value)
            {
                var sheetMusicExists = await _sheetMusicRepository.GetByIdAsync(sheetMusicId.Value);
                if (sheetMusicExists == null)
                {
                    throw new KeyNotFoundException($"Sheet Music with ID {sheetMusicId.Value} not found for update.");
                }
                existingSheet.sheet_music = sheetMusicExists;
            }
        }
        // else if (sheetMusicId == null && existingSheet.sheet_music != null)
        // {
        //     // Nếu bạn muốn có thể gán SheetMusicId về null (nếu DB cho phép),
        //     // bạn sẽ cần code để cập nhật shadow property hoặc FK trực tiếp.
        //     // Tạm thời bỏ qua phần này vì bạn không muốn chỉnh sửa Model.
        // }

        await _sheetRepository.UpdateAsync(existingSheet);
    }

    // Delete Sheet
    public async Task<bool> DeleteAsync(int id)
    {
        var sheetToDelete = await _sheetRepository.GetByIdAsync(id);
        if (sheetToDelete == null)
        {
            return false; // Không tìm thấy để xóa
        }

        // Xóa file ảnh liên quan khỏi Azure Blob trước
        if (!string.IsNullOrEmpty(sheetToDelete.sheet_url))
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(sheetToDelete.sheet_url);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu không xóa được file, nhưng vẫn tiếp tục xóa entity trong DB
                Console.WriteLine($"Error deleting blob for sheet {id}: {ex.Message}");
                // Có thể throw lại lỗi nếu bạn muốn rollback giao dịch
            }
        }

        return await _sheetRepository.DeleteAsync(id);
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