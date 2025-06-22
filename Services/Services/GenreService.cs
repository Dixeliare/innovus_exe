using DTOs;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class GenreService : IGenreService
{
    // private readonly IGenreRepository _genreRepository;
    //
    // public GenreService(IGenreRepository genreService) => _genreRepository = genreService;
    
    private readonly IUnitOfWork _unitOfWork;

    public GenreService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<genre>> GetAllAsync()
    {
        return await _unitOfWork.Genres.GetAllAsync();
    }

    public async Task<genre> GetByIdAsync(int id)
    {
        return await _unitOfWork.Genres.GetByIdAsync(id);
    }

    public async Task<GenreDto> AddAsync(CreateGenreDto createGenreDto)
    {
        var genreEntity = new genre
        {
            genre_name = createGenreDto.GenreName
        };

        var addedGenre = await _unitOfWork.Genres.AddAsync(genreEntity);
        return MapToGenreDto(addedGenre);
    }

    // UPDATE Genre
    public async Task UpdateAsync(UpdateGenreDto updateGenreDto)
    {
        var existingGenre = await _unitOfWork.Genres.GetByIdAsync(updateGenreDto.GenreId);

        if (existingGenre == null)
        {
            throw new KeyNotFoundException($"Genre with ID {updateGenreDto.GenreId} not found.");
        }

        // Cập nhật tên nếu có giá trị được cung cấp
        if (!string.IsNullOrEmpty(updateGenreDto.GenreName))
        {
            existingGenre.genre_name = updateGenreDto.GenreName;
        }
        // Nếu bạn muốn cho phép gán null cho tên thể loại (nếu DB cho phép), bạn có thể thêm:
        // else if (updateGenreDto.GenreName == null)
        // {
        //     existingGenre.genre_name = null;
        // }

        await _unitOfWork.Genres.UpdateAsync(existingGenre);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _unitOfWork.Genres.DeleteAsync(id);
    }

    public async Task<IEnumerable<genre>> SearchGenresAsync(string? genreName = null)
    {
        return await _unitOfWork.Genres.SearchGenresAsync(genreName);
    }
    
    private GenreDto MapToGenreDto(genre model)
    {
        return new GenreDto
        {
            GenreId = model.genre_id,
            GenreName = model.genre_name
        };
    }
}