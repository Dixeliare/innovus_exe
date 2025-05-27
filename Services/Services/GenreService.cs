using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class GenreService : IGenreService
{
    private readonly GenreRepository _genreService;
    
    public GenreService(GenreRepository genreService) => _genreService = genreService;
    
    public async Task<IEnumerable<genre>> GetAllAsync()
    {
        return await _genreService.GetAllAsync();
    }

    public async Task<genre> GetByIdAsync(int id)
    {
        return await _genreService.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(genre genre)
    {
        return await _genreService.CreateAsync(genre);
    }

    public async Task<int> UpdateAsync(genre genre)
    {
        return await _genreService.UpdateAsync(genre);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _genreService.DeleteAsync(id);
    }

    public async Task<IEnumerable<genre>> SearchGenresAsync(string? genreName = null)
    {
        return await _genreService.SearchGenresAsync(genreName);
    }
}