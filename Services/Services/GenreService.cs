using System.Net;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Models;
using Services.Exceptions;
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

    public async Task<IEnumerable<GenreDto>> GetAllAsync()
    {
        var genres = await _unitOfWork.Genres.GetAllAsync();
        return genres.Select(MapToGenreDto);
    }

    public async Task<GenreDto> GetByIdAsync(int id)
    {
        var genre = await _unitOfWork.Genres.GetByIdAsync(id);
        if (genre == null)
        {
            throw new NotFoundException("Genre", "Id", id);
        }
        return MapToGenreDto(genre);
    }

    public async Task<GenreDto> AddAsync(CreateGenreDto createGenreDto)
    {
        // Kiểm tra tên thể loại đã tồn tại chưa (không phân biệt chữ hoa chữ thường)
        var existingGenre = await _unitOfWork.Genres.FindOneAsync(
            g => g.genre_name != null && g.genre_name.ToLower() == createGenreDto.GenreName.ToLower()); // Giả định FindOneAsync có sẵn
        if (existingGenre != null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "GenreName", new string[] { $"Tên thể loại '{createGenreDto.GenreName}' đã tồn tại." } }
            });
        }

        var genreEntity = new genre
        {
            genre_name = createGenreDto.GenreName
        };

        try
        {
            var addedGenre = await _unitOfWork.Genres.AddAsync(genreEntity);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
            return MapToGenreDto(addedGenre);
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi thêm thể loại vào cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while adding the genre.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    // UPDATE Genre
    public async Task UpdateAsync(UpdateGenreDto updateGenreDto)
    {
        var existingGenre = await _unitOfWork.Genres.GetByIdAsync(updateGenreDto.GenreId);

        if (existingGenre == null)
        {
            throw new NotFoundException("Genre", "Id", updateGenreDto.GenreId);
        }

        // Kiểm tra tên thể loại đã tồn tại chưa nếu tên đang được cập nhật và khác biệt
        if (!string.IsNullOrEmpty(updateGenreDto.GenreName) && updateGenreDto.GenreName.ToLower() != existingGenre.genre_name?.ToLower())
        {
            var genreWithSameName = await _unitOfWork.Genres.FindOneAsync(
                g => g.genre_name != null && g.genre_name.ToLower() == updateGenreDto.GenreName.ToLower());
            if (genreWithSameName != null && genreWithSameName.genre_id != updateGenreDto.GenreId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "GenreName", new string[] { $"Tên thể loại '{updateGenreDto.GenreName}' đã được sử dụng bởi một thể loại khác." } }
                });
            }
        }

        // Cập nhật tên nếu có giá trị được cung cấp
        if (updateGenreDto.GenreName != null) // Cho phép null nếu DTO và DB cho phép
        {
            existingGenre.genre_name = updateGenreDto.GenreName;
        }

        try
        {
            await _unitOfWork.Genres.UpdateAsync(existingGenre);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            throw new ApiException("Có lỗi xảy ra khi cập nhật thể loại trong cơ sở dữ liệu.", dbEx, (int)HttpStatusCode.InternalServerError);
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while updating the genre.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var genreToDelete = await _unitOfWork.Genres.GetByIdAsync(id);
        if (genreToDelete == null)
        {
            throw new NotFoundException("Genre", "Id", id);
        }

        try
        {
            await _unitOfWork.Genres.DeleteAsync(id);
            await _unitOfWork.CompleteAsync(); // Lưu thay đổi
        }
        catch (DbUpdateException dbEx)
        {
            // Nếu có bản nhạc nào đó đang liên kết với thể loại này, sẽ ném lỗi FK
            throw new ApiException("Không thể xóa thể loại này vì nó đang được sử dụng bởi một hoặc nhiều bản nhạc.", dbEx, (int)HttpStatusCode.Conflict); // 409 Conflict
        }
        catch (Exception ex)
        {
            throw new ApiException("An unexpected error occurred while deleting the genre.", ex, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IEnumerable<GenreDto>> SearchGenresAsync(string? genreName = null)
    {
        var genres = await _unitOfWork.Genres.SearchGenresAsync(genreName); // Giả định SearchGenresAsync có sẵn
        return genres.Select(MapToGenreDto);
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