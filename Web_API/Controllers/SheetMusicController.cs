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
        private readonly ISheetService _sheetService;
        private readonly IFileStorageService _fileStorageService;

        public SheetMusicController(
            ISheetMusicService sheetMusicService, 
            ISheetService sheetService,
            IFileStorageService fileStorageService)
        {
            _sheetMusicService = sheetMusicService;
            _sheetService = sheetService;
            _fileStorageService = fileStorageService;
        }

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

        // DOWNLOAD: api/SheetMusic/{id}/download-all-sheets
        [HttpGet("{id}/download-all-sheets")]
        public async Task<IActionResult> DownloadAllSheets(int id)
        {
            try
            {
                // Lấy thông tin sheet music
                var sheetMusic = await _sheetMusicService.GetByIdAsync(id);
                if (sheetMusic == null)
                {
                    return NotFound(new { message = "Không tìm thấy bản nhạc" });
                }

                // Lấy tất cả sheets thuộc về bản nhạc này
                var sheets = await _sheetService.GetBySheetMusicIdAsync(id);
                if (!sheets.Any())
                {
                    return BadRequest(new { message = "Bản nhạc này không có sheet nào để download" });
                }

                // Tạo ZIP file chứa tất cả sheets
                var memoryStream = new MemoryStream();
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var sheet in sheets)
                    {
                        try
                        {
                            // Download file từ Azure Blob
                            var fileStream = await _fileStorageService.DownloadFileAsync(sheet.SheetUrl);
                            
                            // Lấy tên file từ URL
                            var sheetFileName = Path.GetFileName(sheet.SheetUrl.Split('?')[0]);
                            
                            // Tạo entry trong ZIP
                            var zipEntry = archive.CreateEntry(sheetFileName);
                            using var entryStream = zipEntry.Open();
                            await fileStream.CopyToAsync(entryStream);
                        }
                        catch (Exception ex)
                        {
                            // Log lỗi nhưng tiếp tục với các file khác
                            Console.WriteLine($"Lỗi khi download sheet {sheet.SheetId}: {ex.Message}");
                        }
                    }
                }

                memoryStream.Position = 0;
                var fileName = $"{sheetMusic.MusicName}_{sheetMusic.Composer}_all_sheets.zip";
                
                // Trả về file và để ASP.NET Core tự động dispose memoryStream
                return File(memoryStream, "application/zip", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi download tất cả sheets", error = ex.Message });
            }
        }

        // GET DOWNLOAD URL: api/SheetMusic/{id}/download-all-sheets-url
        [HttpGet("{id}/download-all-sheets-url")]
        public async Task<IActionResult> GetDownloadAllSheetsUrl(int id, [FromQuery] int? expiryHours = 24)
        {
            try
            {
                // Lấy thông tin sheet music
                var sheetMusic = await _sheetMusicService.GetByIdAsync(id);
                if (sheetMusic == null)
                {
                    return NotFound(new { message = "Không tìm thấy bản nhạc" });
                }

                // Lấy tất cả sheets thuộc về bản nhạc này
                var sheets = await _sheetService.GetBySheetMusicIdAsync(id);
                if (!sheets.Any())
                {
                    return BadRequest(new { message = "Bản nhạc này không có sheet nào để download" });
                }

                var expiry = expiryHours.HasValue ? TimeSpan.FromHours(expiryHours.Value) : TimeSpan.FromHours(24);
                
                // Tạo danh sách download URLs cho từng sheet
                var downloadUrls = new List<object>();
                foreach (var sheet in sheets)
                {
                    try
                    {
                        var downloadUrl = await _fileStorageService.GetDownloadUrlAsync(sheet.SheetUrl, expiry);
                        downloadUrls.Add(new
                        {
                            sheetId = sheet.SheetId,
                            downloadUrl = downloadUrl,
                            fileName = Path.GetFileName(sheet.SheetUrl.Split('?')[0])
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi tạo download URL cho sheet {sheet.SheetId}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    sheetMusicId = id,
                    musicName = sheetMusic.MusicName,
                    composer = sheetMusic.Composer,
                    totalSheets = downloadUrls.Count,
                    expiryHours = expiryHours ?? 24,
                    expiresAt = DateTime.UtcNow.Add(expiry),
                    downloadUrls = downloadUrls
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo download URLs", error = ex.Message });
            }
        }
    }
}
