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
        public async Task<ActionResult<IEnumerable<SheetMusicDto>>> GetAllAsync() // Đổi kiểu trả về thành SheetMusicDto
        {
            var sheetMusics = await _sheetMusicService.GetAllAsync();
            var sheetMusicDtos = sheetMusics.Select(sm => MapToSheetMusicDto(sm));
            return Ok(sheetMusicDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SheetMusicDto>> GetSheetMusicById(int id)
        {
            var sheetMusic = await _sheetMusicService.GetByIdAsync(id);
            if (sheetMusic == null)
            {
                return NotFound();
            }
            return Ok(MapToSheetMusicDto(sheetMusic)); // Dùng hàm MapToSheetMusicDto
        }

        // POST: api/SheetMusics
        [HttpPost]
        // Nhận file từ Form-data. Các thuộc tính khác cũng đi kèm trong form-data.
        public async Task<ActionResult<SheetMusicDto>> CreateSheetMusic([FromForm] CreateSheetMusicDto createSheetMusicDto)
        {
            // Kiểm tra xem file có được gửi lên không
            if (createSheetMusicDto.CoverImageFile == null || createSheetMusicDto.CoverImageFile.Length == 0)
            {
                return BadRequest(new { message = "Cover image file is required." });
            }

            try
            {
                var createdSheetMusic = await _sheetMusicService.AddAsync(
                    createSheetMusicDto.CoverImageFile,
                    createSheetMusicDto.Number,
                    createSheetMusicDto.MusicName,
                    createSheetMusicDto.Composer,
                    createSheetMusicDto.SheetQuantity,
                    createSheetMusicDto.FavoriteCount,
                    createSheetMusicDto.SheetId
                );
                return CreatedAtAction(nameof(GetSheetMusicById), new { id = createdSheetMusic.SheetMusicId }, createdSheetMusic);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex) // Bắt lỗi nếu service báo thiếu file
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
        // Nhận file từ Form-data
        public async Task<IActionResult> UpdateSheetMusic(int id, [FromForm] UpdateSheetMusicDto updateSheetMusicDto)
        {
            if (id != updateSheetMusicDto.SheetMusicId)
            {
                return BadRequest(new { message = "Sheet Music ID in URL does not match ID in body." });
            }

            try
            {
                await _sheetMusicService.UpdateAsync(
                    updateSheetMusicDto.SheetMusicId,
                    updateSheetMusicDto.CoverImageFile, // Có thể là null nếu không upload file mới
                    updateSheetMusicDto.Number,
                    updateSheetMusicDto.MusicName,
                    updateSheetMusicDto.Composer,
                    updateSheetMusicDto.SheetQuantity,
                    updateSheetMusicDto.FavoriteCount,
                    updateSheetMusicDto.SheetId
                );
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
                var result = await _sheetMusicService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Sheet Music with ID {id} not found or could not be deleted." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the sheet music.", error = ex.Message });
            }
        }

        // Search API
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<SheetMusicDto>>> SearchSheetMusic(
            [FromQuery] int? number,
            [FromQuery] string? musicName,
            [FromQuery] string? composer,
            [FromQuery] int? sheetQuantity,
            [FromQuery] int? favoriteCount)
        {
            var sheetMusics = await _sheetMusicService.SearchSheetMusicAsync(number, musicName, composer, sheetQuantity, favoriteCount);
            var sheetMusicDtos = sheetMusics.Select(sm => MapToSheetMusicDto(sm));
            return Ok(sheetMusicDtos);
        }

        [HttpPost("{sheetMusicId}/genres/{genreId}")]
        public async Task<IActionResult> AddGenreToSheetMusic(int sheetMusicId, int genreId)
        {
            try
            {
                await _sheetMusicService.AddGenreToSheetMusicAsync(sheetMusicId, genreId);
                return NoContent();
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

        // Hàm MapToSheetMusicDto chung cho Controller
        private SheetMusicDto MapToSheetMusicDto(sheet_music model)
        {
            return new SheetMusicDto
            {
                SheetMusicId = model.sheet_music_id,
                Number = model.number,
                MusicName = model.music_name,
                Composer = model.composer,
                CoverUrl = model.cover_url,
                SheetQuantity = model.sheet_quantity,
                FavoriteCount = model.favorite_count,
                SheetId = model.sheet_id
            };
        }
    }
}
