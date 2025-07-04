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
    public class UserFavoriteSheetController : ControllerBase
    {
        private readonly IUserFavoriteSheetService _userFavoriteSheetService;
        
        public UserFavoriteSheetController(IUserFavoriteSheetService userFavoriteSheetService) => _userFavoriteSheetService = userFavoriteSheetService;

        [HttpGet("{userId}/favorites")]
        public async Task<ActionResult<IEnumerable<sheet_music>>> GetFavoriteSheetsByUserAsync(int userId)
        {
            // Service sẽ ném NotFoundException nếu userId không tồn tại
            var favoriteSheets = await _userFavoriteSheetService.GetFavoriteSheetsByUserAsync(userId);
            return Ok(favoriteSheets);
        }

        [HttpGet("{sheetMusicId}/favoritingUsers")]
        public async Task<ActionResult<IEnumerable<user>>> GetUsersFavoritingSheetAsync(int sheetMusicId)
        {
            // Service sẽ ném NotFoundException nếu sheetMusicId không tồn tại
            var users = await _userFavoriteSheetService.GetUsersFavoritingSheetAsync(sheetMusicId);
            return Ok(users);
        }

        [HttpGet("{userId}/{sheetMusicId}/isFavorite")]
        public async Task<ActionResult<bool>> CheckIfSheetIsFavoriteForUserAsync(int userId, int sheetMusicId)
        {
            // Service sẽ tự kiểm tra sự tồn tại của User/SheetMusic nếu cần
            var isFavorite = await _userFavoriteSheetService.CheckIfSheetIsFavoriteForUserAsync(userId, sheetMusicId);
            return Ok(isFavorite);
        }
        
        [HttpGet("{userId}/{sheetMusicId}")]
        public async Task<ActionResult<UserFavoriteSheetDto>> GetUserFavoriteSheetById(int userId, int sheetMusicId)
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var favoriteSheet = await _userFavoriteSheetService.GetByIdAsync(userId, sheetMusicId);
            return Ok(favoriteSheet);
        }

        [HttpPost]
        public async Task<ActionResult<UserFavoriteSheetDto>> AddUserFavoriteSheet([FromBody] CreateUserFavoriteSheetDto createDto)
        {
            // Không còn try-catch ở đây, để Middleware xử lý exception
            var createdFavorite = await _userFavoriteSheetService.AddUserFavoriteSheetAsync(createDto);
            // Trả về 201 Created và vị trí của tài nguyên mới tạo
            return CreatedAtAction(nameof(GetUserFavoriteSheetById), // Thay đổi tên hàm để khớp với Get cho cặp ID
                new { userId = createdFavorite.UserId, sheetMusicId = createdFavorite.SheetMusicId },
                createdFavorite);
        }

        // PUT: api/UserFavoriteSheets/{userId}/{sheetMusicId}
        // Endpoint này cập nhật trạng thái is_favorite của một mục yêu thích đã tồn tại.
        [HttpPut("{userId}/{sheetMusicId}")]
        public async Task<IActionResult> UpdateUserFavoriteSheet(int userId, int sheetMusicId, [FromBody] UpdateUserFavoriteSheetDto updateDto)
        {
            if (userId != updateDto.UserId || sheetMusicId != updateDto.SheetMusicId)
            {
                // Ném ValidationException thay vì BadRequest
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "IdMismatch", new string[] { "User ID hoặc Sheet Music ID trong URL không khớp với ID trong body." } }
                });
            }

            // Không còn try-catch ở đây
            await _userFavoriteSheetService.UpdateUserFavoriteSheetAsync(updateDto);
            return NoContent(); // 204 No Content for successful update
        }

        [HttpDelete("{userId}/{sheetMusicId}")]
        public async Task<IActionResult> DeleteUserFavoriteSheetAsync([FromQuery] int userId,[FromQuery]  int sheetMusicId)
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            await _userFavoriteSheetService.DeleteUserFavoriteSheetAsync(userId, sheetMusicId);
            return NoContent(); // 204 No Content for successful deletion
        }
    }
}
