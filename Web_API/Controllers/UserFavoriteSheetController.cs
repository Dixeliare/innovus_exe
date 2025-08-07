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
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    public class UserFavoriteSheetController : BaseController
    {
        private readonly IUserFavoriteSheetService _userFavoriteSheetService;
        private readonly IUserService _userService;

        public UserFavoriteSheetController(IUserFavoriteSheetService userFavoriteSheetService, IUserService userService)
        {
            _userFavoriteSheetService = userFavoriteSheetService;
            _userService = userService;
        }



        // GET: api/UserFavoriteSheet/my-favorites
        [HttpGet("my-favorites")]
        [Authorize]
        public async Task<ActionResult<UserFavoriteSheetListDto>> GetMyFavorites()
        {
            var userId = GetCurrentUserId();
            var userFavorites = await _userFavoriteSheetService.GetUserFavoritesAsync(userId);
            return Ok(userFavorites);
        }



        // GET: api/UserFavoriteSheet/check-my-favorite/{sheetMusicId}
        [HttpGet("check-my-favorite/{sheetMusicId}")]
        [Authorize]
        public async Task<ActionResult<bool>> CheckMyFavorite(int sheetMusicId)
        {
            var userId = GetCurrentUserId();
            var isFavorite = await _userFavoriteSheetService.IsFavoriteAsync(userId, sheetMusicId);
            return Ok(isFavorite);
        }

        // POST: api/UserFavoriteSheet/toggle/{sheetMusicId}
        [HttpPost("toggle/{sheetMusicId}")]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int sheetMusicId)
        {
            var userId = GetCurrentUserId();
            var wasFavorite = await _userFavoriteSheetService.IsFavoriteAsync(userId, sheetMusicId);
            await _userFavoriteSheetService.ToggleFavoriteAsync(userId, sheetMusicId);
            
            var newStatus = !wasFavorite;
            var message = newStatus ? "Sheet music liked successfully" : "Sheet music unliked successfully";
            
            return Ok(new { 
                message = message,
                isFavorite = newStatus
            });
        }
    }
}