using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;
using Services.Exceptions;
using Services.IServices;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    public class GenreController : BaseController
    {
        private readonly IGenreService _genreService;
        
        public GenreController(IGenreService genreService) => _genreService = genreService;

        [HttpGet("search_by_genre_name")]
        public async Task<ActionResult<IEnumerable<GenreDto>>> SearchGenresAsync([FromQuery] string? genreName = null)
        {
            var genres = await _genreService.SearchGenresAsync(genreName);
            return Ok(genres); // Service đã trả về DTOs
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreDto>>> GetAllAsync() // Trả về DTOs
        {
            var genres = await _genreService.GetAllAsync();
            return Ok(genres);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenreDto>> GetGenreById(int id) // Trả về DTO
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var genre = await _genreService.GetByIdAsync(id);
            return Ok(genre); // Service đã trả về DTO
        }

        // POST: api/Genres
        [HttpPost]
        public async Task<ActionResult<GenreDto>> CreateGenre([FromBody] CreateGenreDto createGenreDto)
        {
            // Không có try-catch ở đây. Service sẽ ném ValidationException/ApiException nếu có lỗi.
            var createdGenre = await _genreService.AddAsync(createGenreDto);
            return CreatedAtAction(nameof(GetGenreById), new { id = createdGenre.GenreId }, createdGenre);
        }

        // PUT: api/Genres/{id}
        [HttpPut("{id}")]
        // PUT: api/Genres/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] UpdateGenreDto updateGenreDto)
        {
            if (id != updateGenreDto.GenreId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "GenreId", new string[] { "ID thể loại trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            await _genreService.UpdateAsync(updateGenreDto);
            return NoContent();
        }

        // DELETE: api/Genres/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ApiException nếu có lỗi.
            await _genreService.DeleteAsync(id);
            return NoContent();
        }


    }
}
