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
    public class SheetMusicController : BaseController
    {
        private readonly ISheetMusicService _sheetMusicService;

        public SheetMusicController(ISheetMusicService sheetMusicService) => _sheetMusicService = sheetMusicService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SheetMusicDto>>> GetAllAsync() // Đổi kiểu trả về thành SheetMusicDto
        {
            var sheetMusics = await _sheetMusicService.GetAllAsync();
            return Ok(sheetMusics);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SheetMusicDto>> GetSheetMusicById(int id)
        {
            var sheetMusic = await _sheetMusicService.GetByIdAsync(id);
            return Ok(sheetMusic);
        }

        // POST: api/SheetMusics
        [HttpPost]
        // Nhận file từ Form-data. Các thuộc tính khác cũng đi kèm trong form-data.
        public async Task<ActionResult<SheetMusicDto>> CreateSheetMusic([FromForm] CreateSheetMusicDto createSheetMusicDto)
        {
            // Service sẽ tự kiểm tra createSheetMusicDto.CoverImageFile và ném ValidationException nếu cần
            var createdSheetMusic = await _sheetMusicService.AddAsync(
                createSheetMusicDto.CoverImageFile,
                createSheetMusicDto.Number,
                createSheetMusicDto.MusicName,
                createSheetMusicDto.Composer,
                createSheetMusicDto.SheetQuantity,
                createSheetMusicDto.FavoriteCount,
                createSheetMusicDto.GenreIds
            );
            return CreatedAtAction(nameof(GetSheetMusicById), new { id = createdSheetMusic.SheetMusicId }, createdSheetMusic);
        }

        // PUT: api/SheetMusics/{id}
        [HttpPut("{id}")]
        // Nhận file từ Form-data
        public async Task<ActionResult<SheetMusicDto>> UpdateSheetMusic(int id, [FromForm] UpdateSheetMusicDto updateSheetMusicDto)
        {
            if (id != updateSheetMusicDto.SheetMusicId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "SheetMusicId", new string[] { "ID bản nhạc trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây
            await _sheetMusicService.UpdateAsync(
                updateSheetMusicDto.SheetMusicId,
                updateSheetMusicDto.CoverImageFile, // Có thể là null nếu không upload file mới
                updateSheetMusicDto.Number,
                updateSheetMusicDto.MusicName,
                updateSheetMusicDto.Composer,
                updateSheetMusicDto.SheetQuantity,
                updateSheetMusicDto.FavoriteCount,
                updateSheetMusicDto.GenreIds
            );
            
            // Trả về data đã cập nhật với đầy đủ genres
            var updatedSheetMusic = await _sheetMusicService.GetByIdAsync(id);
            return Ok(updatedSheetMusic);
        }

        // DELETE: api/SheetMusics/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSheetMusic(int id)
        {
            // Không có try-catch ở đây
            await _sheetMusicService.DeleteAsync(id);
            return NoContent();
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
            return Ok(sheetMusics); // Service đã trả về DTO rồi
        }

        [HttpPost("{sheetMusicId}/genres/{genreId}")]
        public async Task<ActionResult<SheetMusicDto>> AddGenreToSheetMusic(int sheetMusicId, int genreId)
        {
            // Không có try-catch ở đây
            await _sheetMusicService.AddGenreToSheetMusicAsync(sheetMusicId, genreId);
            
            // Trả về sheet music đã cập nhật với genres
            var updatedSheetMusic = await _sheetMusicService.GetByIdAsync(sheetMusicId);
            return Ok(updatedSheetMusic);
        }

        [HttpDelete("{sheetMusicId}/genres/{genreId}")]
        public async Task<ActionResult<SheetMusicDto>> RemoveGenreFromSheetMusic(int sheetMusicId, int genreId)
        {
            // Không có try-catch ở đây
            await _sheetMusicService.RemoveGenreFromSheetMusicAsync(sheetMusicId, genreId);
            
            // Trả về sheet music đã cập nhật với genres
            var updatedSheetMusic = await _sheetMusicService.GetByIdAsync(sheetMusicId);
            return Ok(updatedSheetMusic);
        }
    }
}
