using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IGenreRepository
{
    Task<IEnumerable<genre>> GetAllAsync();
    Task<genre> GetByIdAsync(int id);
    Task<genre> AddAsync(genre entity);
    Task UpdateAsync(genre entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<genre>> SearchGenresAsync(string? genreName = null);
}