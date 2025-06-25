using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IGenreService
{
    Task<IEnumerable<GenreDto>> GetAllAsync();
    Task<GenreDto> GetByIdAsync(int id);
    Task<GenreDto> AddAsync(CreateGenreDto createGenreDto);
    Task UpdateAsync(UpdateGenreDto updateGenreDto);
    Task DeleteAsync(int id);
    Task<IEnumerable<GenreDto>> SearchGenresAsync(string? genreName = null);
}