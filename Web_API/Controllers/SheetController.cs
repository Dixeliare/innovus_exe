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
        private readonly IFileStorageService _fileStorageService;

        public SheetController(ISheetService sheetService, IFileStorageService fileStorageService)
        {
            _sheetService = sheetService;
            _fileStorageService = fileStorageService;
        }

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

        // DOWNLOAD: api/Sheets/{id}/download
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadSheet(int id)
        {
            try
            {
                // Lấy thông tin sheet
                var sheet = await _sheetService.GetByIdAsync(id);
                if (sheet == null)
                {
                    return NotFound(new { message = "Không tìm thấy sheet nhạc" });
                }

                // Kiểm tra có file URL không
                if (string.IsNullOrEmpty(sheet.SheetUrl))
                {
                    return BadRequest(new { message = "Sheet nhạc không có file để download" });
                }

                // Download file từ Azure Blob
                var fileStream = await _fileStorageService.DownloadFileAsync(sheet.SheetUrl);
                
                // Lấy tên file từ URL
                var fileName = Path.GetFileName(sheet.SheetUrl.Split('?')[0]);
                
                // Xác định content type
                var contentType = GetContentType(fileName);
                
                return File(fileStream, contentType, fileName);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = "File không tồn tại trên Azure Blob", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi download sheet nhạc", error = ex.Message });
            }
        }

        // GET DOWNLOAD URL: api/Sheets/{id}/download-url
        [HttpGet("{id}/download-url")]
        public async Task<IActionResult> GetDownloadUrl(int id, [FromQuery] int? expiryHours = 1)
        {
            try
            {
                // Lấy thông tin sheet
                var sheet = await _sheetService.GetByIdAsync(id);
                if (sheet == null)
                {
                    return NotFound(new { message = "Không tìm thấy sheet nhạc" });
                }

                // Kiểm tra có file URL không
                if (string.IsNullOrEmpty(sheet.SheetUrl))
                {
                    return BadRequest(new { message = "Sheet nhạc không có file để download" });
                }

                var expiry = expiryHours.HasValue ? TimeSpan.FromHours(expiryHours.Value) : TimeSpan.FromHours(1);
                var downloadUrl = await _fileStorageService.GetDownloadUrlAsync(sheet.SheetUrl, expiry);
                
                return Ok(new { 
                    downloadUrl = downloadUrl,
                    expiryHours = expiryHours ?? 1,
                    expiresAt = DateTime.UtcNow.Add(expiry),
                    fileName = Path.GetFileName(sheet.SheetUrl.Split('?')[0])
                });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = "File không tồn tại trên Azure Blob", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo download URL", error = ex.Message });
            }
        }

        // Helper method để xác định content type
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".mp3" => "audio/mpeg",
                ".mp4" => "video/mp4",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        }
    }
}
