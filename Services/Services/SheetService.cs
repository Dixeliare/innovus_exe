using DTOs;
using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class SheetService : ISheetService
{
    private readonly SheetRepository _sheetRepository;
    private readonly SheetMusicRepository _sheetMusicRepository; // Inject cho kiểm tra khóa ngoại sheet_music

    public SheetService(SheetRepository sheetRepository,
        SheetMusicRepository sheetMusicRepository)
    {
        _sheetRepository = sheetRepository;
        _sheetMusicRepository = sheetMusicRepository;
    }
    
    public async Task<IEnumerable<sheet>> GetAllAsync()
    {
        return await _sheetRepository.GetAllAsync();
    }

    public async Task<sheet> GetByIdAsync(int id)
    {
        return await _sheetRepository.GetByIdAsync(id);
    }

    public async Task<SheetDto> AddAsync(CreateSheetDto createSheetDto)
        {
            // Kiểm tra sự tồn tại của khóa ngoại SheetMusic
            var sheetMusicExists = await _sheetMusicRepository.GetByIdAsync(createSheetDto.SheetMusicId);
            if (sheetMusicExists == null)
            {
                throw new KeyNotFoundException($"Sheet Music with ID {createSheetDto.SheetMusicId} not found.");
            }

            var sheetEntity = new sheet
            {
                sheet_url = createSheetDto.SheetUrl,
                // Gán navigation property, EF Core sẽ tự động thiết lập khóa ngoại
                sheet_music = sheetMusicExists // Gán trực tiếp entity
            };

            var addedSheet = await _sheetRepository.AddAsync(sheetEntity);
            return MapToSheetDto(addedSheet);
        }

        // UPDATE Sheet
        public async Task UpdateAsync(UpdateSheetDto updateSheetDto)
        {
            var existingSheet = await _sheetRepository.GetByIdAsync(updateSheetDto.SheetId);

            if (existingSheet == null)
            {
                throw new KeyNotFoundException($"Sheet with ID {updateSheetDto.SheetId} not found.");
            }

            // Cập nhật các trường nếu có giá trị được cung cấp
            if (!string.IsNullOrEmpty(updateSheetDto.SheetUrl))
            {
                existingSheet.sheet_url = updateSheetDto.SheetUrl;
            }

            // Kiểm tra và cập nhật khóa ngoại Sheet Music nếu có giá trị mới được cung cấp
            if (updateSheetDto.SheetMusicId.HasValue)
            {
                // Tránh cập nhật nếu ID không đổi (hoặc cả hai đều null)
                if (existingSheet.sheet_music?.sheet_music_id != updateSheetDto.SheetMusicId.Value)
                {
                    var sheetMusicExists = await _sheetMusicRepository.GetByIdAsync(updateSheetDto.SheetMusicId.Value);
                    if (sheetMusicExists == null)
                    {
                        throw new KeyNotFoundException($"Sheet Music with ID {updateSheetDto.SheetMusicId} not found for update.");
                    }
                    // Gán navigation property mới
                    existingSheet.sheet_music = sheetMusicExists;
                }
            }
            // Nếu SheetMusicId được gửi là null, xóa liên kết (nếu FK trong DB cho phép null)
            // Điều này có thể phức tạp với shadow properties, cần kiểm tra cấu hình DB First của bạn.
            // Nếu DB First model có "sheet_music_id" và nó là nullable, thì set nó bằng null
            // Để đơn giản, nếu sheet_music_id được gửi là null và existingSheet có liên kết, sẽ bỏ qua
            // Nếu muốn rõ ràng set null:
            // else if (updateSheetDto.SheetMusicId == null && existingSheet.sheet_music != null)
            // {
            //     existingSheet.sheet_music = null; // hoặc gán shadow property về null
            // }


            await _sheetRepository.UpdateAsync(existingSheet);
        }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _sheetRepository.DeleteAsync(id);
    }
    
    private SheetDto MapToSheetDto(sheet model)
    {
        return new SheetDto
        {
            SheetId = model.sheet_id,
            SheetUrl = model.sheet_url,
            // Giả định có một shadow property hoặc đã được cấu hình trong AppDbContext để lấy sheet_music_id
            // Nếu không, bạn cần điều chỉnh cách lấy SheetMusicId từ model
            SheetMusicId = model.sheet_music?.sheet_music_id ?? 0 // Cẩn thận với giá trị mặc định 0 nếu không có liên kết
        };
    }
}