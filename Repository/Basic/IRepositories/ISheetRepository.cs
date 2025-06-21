using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface ISheetRepository
{
    Task<IEnumerable<sheet>> GetAllAsync();
    Task<sheet> GetByIdAsync(int id);
    Task<sheet> AddAsync(sheet entity);
    Task UpdateAsync(sheet entity);
    Task<bool> DeleteAsync(int id);
}