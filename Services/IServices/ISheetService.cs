using DTOs;
using Microsoft.AspNetCore.Http;
using Repository.Models;

namespace Services.IServices;

public interface ISheetService
{
    // Task<IEnumerable<sheet>> GetAllAsync();
    // Task<sheet> GetByIdAsync(int id);
    // Task<SheetDto> AddAsync(CreateSheetDto createSheetDto);
    // Task UpdateAsync(UpdateSheetDto updateSheetDto);
    // Task<bool> DeleteAsync(int id);
    
    Task<IEnumerable<SheetDto>> GetAllAsync();
    Task<SheetDto> GetByIdAsync(int id);
    // Thay đổi AddAsync để nhận IFormFile
    Task<SheetDto> AddAsync(IFormFile sheetFile, int sheetMusicId); // Hoặc CreateSheetDto nếu bạn muốn giữ DTO

    // Thay đổi UpdateAsync để nhận IFormFile
    Task UpdateAsync(int sheetId, IFormFile? sheetFile, int? sheetMusicId); // Hoặc UpdateSheetDto nếu bạn muốn giữ DTO

    Task DeleteAsync(int id);
}