using Repository.Models;

namespace Services.IServices;

public interface IGenreService
{
    Task<IEnumerable<genre>> GetAllAsync();
    Task<genre> GetByIdAsync(int id);
    Task<int> CreateAsync(genre genre);
    Task<int> UpdateAsync(genre genre);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<genre>> SearchGenresAsync(string? genreName = null);
}