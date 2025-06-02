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
    public class SheetMusicController : ControllerBase
    {
        private readonly ISheetMusicService _sheetMusicService;
        
        public SheetMusicController(ISheetMusicService sheetMusicService) => _sheetMusicService = sheetMusicService;

        [HttpGet]
        public async Task<IEnumerable<sheet_music>> GetAllAsync()
        {
            return await _sheetMusicService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SheetMusicDto>> GetSheetMusicById(int id)
        {
            var sheetMusic = await _sheetMusicService.GetByIdAsync(id);
            if (sheetMusic == null)
            {
                return NotFound();
            }
            return Ok(sheetMusic);
        }

        // POST: api/SheetMusics
        [HttpPost]
        public async Task<ActionResult<SheetMusicDto>> CreateSheetMusic([FromBody] CreateSheetMusicDto createSheetMusicDto)
        {
            try
            {
                var createdSheetMusic = await _sheetMusicService.AddAsync(createSheetMusicDto);
                return CreatedAtAction(nameof(GetSheetMusicById), new { id = createdSheetMusic.SheetMusicId }, createdSheetMusic);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the sheet music.", error = ex.Message });
            }
        }

        // PUT: api/SheetMusics/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSheetMusic(int id, [FromBody] UpdateSheetMusicDto updateSheetMusicDto)
        {
            if (id != updateSheetMusicDto.SheetMusicId)
            {
                return BadRequest(new { message = "Sheet Music ID in URL does not match ID in body." });
            }

            try
            {
                await _sheetMusicService.UpdateAsync(updateSheetMusicDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the sheet music.", error = ex.Message });
            }
        }

        // DELETE: api/SheetMusics/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSheetMusic(int id)
        {
            try
            {
                await _sheetMusicService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the sheet music.", error = ex.Message });
            }
        }
        
        [HttpPost("{sheetMusicId}/genres/{genreId}")]
        public async Task<IActionResult> AddGenreToSheetMusic(int sheetMusicId, int genreId)
        {
            try
            {
                await _sheetMusicService.AddGenreToSheetMusicAsync(sheetMusicId, genreId);
                return NoContent(); // 204 No Content for successful operation without return value
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding genre to sheet music.", error = ex.Message });
            }
        }

        // DELETE: api/SheetMusics/{sheetMusicId}/genres/{genreId} - Remove a genre from sheet music
        [HttpDelete("{sheetMusicId}/genres/{genreId}")]
        public async Task<IActionResult> RemoveGenreFromSheetMusic(int sheetMusicId, int genreId)
        {
            try
            {
                await _sheetMusicService.RemoveGenreFromSheetMusicAsync(sheetMusicId, genreId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing genre from sheet music.", error = ex.Message });
            }
        }
    }
}
