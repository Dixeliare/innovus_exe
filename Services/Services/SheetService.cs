using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class SheetService : ISheetService
{
    private readonly SheetRepository _sheetRepository;
    
    public SheetService(SheetRepository sheetRepository) => _sheetRepository = sheetRepository;
    
    public async Task<IEnumerable<sheet>> GetAllAsync()
    {
        return await _sheetRepository.GetAllAsync();
    }

    public async Task<sheet> GetByIdAsync(int id)
    {
        return await _sheetRepository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(sheet sheet)
    {
        return await _sheetRepository.CreateAsync(sheet);
    }

    public async Task<int> UpdateAsync(sheet sheet)
    {
        return await _sheetRepository.UpdateAsync(sheet);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _sheetRepository.DeleteAsync(id);
    }
}