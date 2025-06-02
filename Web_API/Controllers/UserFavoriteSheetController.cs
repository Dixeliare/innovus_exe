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
    public class UserFavoriteSheetController : ControllerBase
    {
        private readonly IUserFavoriteSheetService _userFavoriteSheetService;
        
        public UserFavoriteSheetController(IUserFavoriteSheetService userFavoriteSheetService) => _userFavoriteSheetService = userFavoriteSheetService;

        [HttpGet("{userId}")]
        public async Task<IEnumerable<sheet_music>> GetFavoriteSheetsByUserAsync([FromQuery] int userId)
        {
            return await _userFavoriteSheetService.GetFavoriteSheetsByUserAsync(userId);
        }

        [HttpGet("{sheetMusicId}/users")]
        public async Task<IEnumerable<user>> GetUsersFavoritingSheetAsync([FromQuery] int sheetMusicId)
        {
            return await _userFavoriteSheetService.GetUsersFavoritingSheetAsync(sheetMusicId);
        }

        [HttpGet("{userId}/{sheetMusicId}")]
        public async Task<bool> CheckIfSheetIsFavoriteForUserAsync([FromQuery] int userId,[FromQuery]  int sheetMusicId)
        {
            return await _userFavoriteSheetService.CheckIfSheetIsFavoriteForUserAsync(userId, sheetMusicId);
        }

        [HttpPost]
        public async Task<ActionResult<UserFavoriteSheetDto>> AddUserFavoriteSheet([FromBody] CreateUserFavoriteSheetDto createDto)
        {
            try
            {
                var createdFavorite = await _userFavoriteSheetService.AddUserFavoriteSheetAsync(createDto);
                // Trả về 201 Created và vị trí của tài nguyên mới tạo
                return CreatedAtAction(nameof(CheckIfSheetIsFavoriteForUserAsync),
                                       new { userId = createdFavorite.UserId, sheetMusicId = createdFavorite.SheetMusicId },
                                       createdFavorite);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // User hoặc Sheet Music không tồn tại
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message }); // Đã tồn tại mục yêu thích này
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the user favorite sheet.", error = ex.Message });
            }
        }

        // PUT: api/UserFavoriteSheets/{userId}/{sheetMusicId}
        // Endpoint này cập nhật trạng thái is_favorite của một mục yêu thích đã tồn tại.
        [HttpPut("{userId}/{sheetMusicId}")]
        public async Task<IActionResult> UpdateUserFavoriteSheet(int userId, int sheetMusicId, [FromBody] UpdateUserFavoriteSheetDto updateDto)
        {
            if (userId != updateDto.UserId || sheetMusicId != updateDto.SheetMusicId)
            {
                return BadRequest(new { message = "User ID or Sheet Music ID in URL does not match IDs in body." });
            }

            try
            {
                await _userFavoriteSheetService.UpdateUserFavoriteSheetAsync(updateDto);
                return NoContent(); // 204 No Content for successful update
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the user favorite sheet.", error = ex.Message });
            }
        }

        [HttpDelete("{userId}/{sheetMusicId}")]
        public async Task<bool> DeleteUserFavoriteSheetAsync([FromQuery] int userId,[FromQuery]  int sheetMusicId)
        {
            return await _userFavoriteSheetService.DeleteUserFavoriteSheetAsync(userId, sheetMusicId);
        }
    }
}
