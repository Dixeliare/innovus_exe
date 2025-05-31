using Repository.Models;

namespace Services.IServices;

public interface ISheetService
{
    Task<IEnumerable<sheet>> GetAllAsync();
    Task<sheet> GetByIdAsync(int id);
    Task<int> CreateAsync(sheet sheet);
    Task<int> UpdateAsync(sheet sheet);
    Task<bool> DeleteAsync(int id);
}