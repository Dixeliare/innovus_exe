using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IGenreService
{
    Task<IEnumerable<genre>> GetAllAsync();
    Task<genre> GetByIdAsync(int id);
    Task<GenreDto> AddAsync(CreateGenreDto createGenreDto);
    Task UpdateAsync(UpdateGenreDto updateGenreDto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<genre>> SearchGenresAsync(string? genreName = null);
}