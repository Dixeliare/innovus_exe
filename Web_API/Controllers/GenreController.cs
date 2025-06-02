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
using Services.IServices;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;
        
        public GenreController(IGenreService genreService) => _genreService = genreService;

        [HttpGet("search_by_genre_name")]
        public async Task<IEnumerable<genre>> SearchGenresAsync([FromQuery ]string? genreName = null)
        {
            return await _genreService.SearchGenresAsync(genreName);
        }

        [HttpGet]
        public async Task<IEnumerable<genre>> GetAll()
        {
            return await _genreService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenreDto>> GetGenreById(int id)
        {
            var genre = await _genreService.GetByIdAsync(id);
            if (genre == null)
            {
                return NotFound();
            }
            return Ok(genre);
        }

        // POST: api/Genres
        [HttpPost]
        public async Task<ActionResult<GenreDto>> CreateGenre([FromBody] CreateGenreDto createGenreDto)
        {
            try
            {
                var createdGenre = await _genreService.AddAsync(createGenreDto);
                return CreatedAtAction(nameof(GetGenreById), new { id = createdGenre.GenreId }, createdGenre);
            }
            catch (Exception ex)
            {
                // Vì không có khóa ngoại, lỗi thường là do validation hoặc DB
                return StatusCode(500, new { message = "An error occurred while creating the genre.", error = ex.Message });
            }
        }

        // PUT: api/Genres/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] UpdateGenreDto updateGenreDto)
        {
            if (id != updateGenreDto.GenreId)
            {
                return BadRequest(new { message = "Genre ID in URL does not match ID in body." });
            }

            try
            {
                await _genreService.UpdateAsync(updateGenreDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the genre.", error = ex.Message });
            }
        }

        // DELETE: api/Genres/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            try
            {
                await _genreService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the genre.", error = ex.Message });
            }
        }

    }
}
