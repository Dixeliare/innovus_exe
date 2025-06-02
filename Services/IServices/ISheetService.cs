using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface ISheetService
{
    Task<IEnumerable<sheet>> GetAllAsync();
    Task<sheet> GetByIdAsync(int id);
    Task<SheetDto> AddAsync(CreateSheetDto createSheetDto);
    Task UpdateAsync(UpdateSheetDto updateSheetDto);
    Task<bool> DeleteAsync(int id);
}