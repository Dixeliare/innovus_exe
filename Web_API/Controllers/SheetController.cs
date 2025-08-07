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
    [ApiController]
    [Produces("application/json")]
    public class SheetController : ControllerBase
    {
        private readonly ISheetService _sheetService;

        public SheetController(ISheetService sheetService) => _sheetService = sheetService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SheetDto>>> GetAllAsync() // Đổi kiểu trả về thành SheetDto
        {
            var sheets = await _sheetService.GetAllAsync();
            return Ok(sheets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SheetDto>> GetSheetById(int id)
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var sheet = await _sheetService.GetByIdAsync(id);
            return Ok(sheet);
        }

        // POST: api/Sheets
        [HttpPost]
        // Nhận file từ Form-data
        public async Task<ActionResult<SheetDto>> CreateSheet([FromForm] CreateSheetDto createSheetDto)
        {
            // Xác thực cơ bản cho sự hiện diện của tệp, xác thực mạnh mẽ hơn trong service
            // Service sẽ ném ValidationException nếu tệp là null/trống
            var createdSheet = await _sheetService.AddAsync(
                createSheetDto.SheetFile,
                createSheetDto.SheetMusicId
            );
            return CreatedAtAction(nameof(GetSheetById), new { id = createdSheet.SheetId }, createdSheet);
        }

        // PUT: api/Sheets/{id}
        [HttpPut("{id}")]
        // Nhận file từ Form-data (sử dụng FromForm cho cả DTO)
        public async Task<IActionResult> UpdateSheet(int id, [FromForm] UpdateSheetDto updateSheetDto)
        {
            if (id != updateSheetDto.SheetId)
            {
                // Ném ValidationException thay vì BadRequest
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "SheetId", new string[] { "ID bản nhạc trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây, service sẽ ném NotFoundException/ValidationException/ApiException
            await _sheetService.UpdateAsync(
                updateSheetDto.SheetId,
                updateSheetDto.SheetFile, // Có thể là null nếu không tải lên tệp mới
                updateSheetDto.SheetMusicId
            );
            return NoContent(); // 204 No Content cho cập nhật thành công
        }

        // DELETE: api/Sheets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSheet(int id)
        {
            // Không có try-catch ở đây, service sẽ ném NotFoundException/ApiException
            await _sheetService.DeleteAsync(id);
            return NoContent(); // 204 No Content cho xóa thành công
        }
    }
}
