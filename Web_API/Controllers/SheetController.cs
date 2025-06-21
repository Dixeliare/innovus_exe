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
    public class SheetController : ControllerBase
    {
        private readonly ISheetService _sheetService;

        public SheetController(ISheetService sheetService) => _sheetService = sheetService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SheetDto>>> GetAllAsync() // Đổi kiểu trả về thành SheetDto
        {
            var sheets = await _sheetService.GetAllAsync();
            // Map từ model sang DTO để trả về
            var sheetDtos = sheets.Select(s => new SheetDto
            {
                SheetId = s.sheet_id,
                SheetUrl = s.sheet_url,
                SheetMusicId = s.sheet_music?.sheet_music_id ?? 0 // Cẩn thận với giá trị 0 nếu null
            });
            return Ok(sheetDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SheetDto>> GetSheetById(int id)
        {
            var sheet = await _sheetService.GetByIdAsync(id);
            if (sheet == null)
            {
                return NotFound();
            }
            return Ok(MapToSheetDto(sheet)); // Dùng hàm MapToSheetDto
        }

        // POST: api/Sheets
        [HttpPost]
        // Nhận file từ Form-data
        public async Task<ActionResult<SheetDto>> CreateSheet([FromForm] CreateSheetDto createSheetDto)
        {
            // Kiểm tra xem file có được gửi lên không
            if (createSheetDto.SheetFile == null || createSheetDto.SheetFile.Length == 0)
            {
                return BadRequest(new { message = "Sheet file is required." });
            }

            try
            {
                var createdSheet = await _sheetService.AddAsync(
                    createSheetDto.SheetFile,
                    createSheetDto.SheetMusicId
                );
                return CreatedAtAction(nameof(GetSheetById), new { id = createdSheet.SheetId }, createdSheet);
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
                return StatusCode(500, new { message = "An error occurred while creating the sheet.", error = ex.Message });
            }
        }

        // PUT: api/Sheets/{id}
        [HttpPut("{id}")]
        // Nhận file từ Form-data (sử dụng FromForm cho cả DTO)
        public async Task<IActionResult> UpdateSheet(int id, [FromForm] UpdateSheetDto updateSheetDto)
        {
            if (id != updateSheetDto.SheetId)
            {
                return BadRequest(new { message = "Sheet ID in URL does not match ID in body." });
            }

            try
            {
                await _sheetService.UpdateAsync(
                    updateSheetDto.SheetId,
                    updateSheetDto.SheetFile, // Có thể là null nếu không upload file mới
                    updateSheetDto.SheetMusicId
                );
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the sheet.", error = ex.Message });
            }
        }

        // DELETE: api/Sheets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSheet(int id)
        {
            try
            {
                var result = await _sheetService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Sheet with ID {id} not found or could not be deleted." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the sheet.", error = ex.Message });
            }
        }

        // Hàm MapToSheetDto chung cho Controller
        private SheetDto MapToSheetDto(sheet model)
        {
            return new SheetDto
            {
                SheetId = model.sheet_id,
                SheetUrl = model.sheet_url,
                SheetMusicId = model.sheet_music?.sheet_music_id ?? 0
            };
        }
    }
}
