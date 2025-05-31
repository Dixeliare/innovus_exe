using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [HttpGet("{sheetMusicId}")]
        public async Task<user_favorite_sheet?> GetUserFavoriteSheetEntryAsync([FromQuery] int userId,[FromQuery]  int sheetMusicId)
        {
            return await _userFavoriteSheetService.GetUserFavoriteSheetEntryAsync(userId, sheetMusicId);
        }

        [HttpPost]
        public async Task<bool> AddUserFavoriteSheetAsync(int userId, int sheetMusicId, bool isFavorite = true)
        {
            return await _userFavoriteSheetService.AddUserFavoriteSheetAsync(userId, sheetMusicId, isFavorite);
        }

        [HttpPut]
        public async Task<bool> UpdateUserFavoriteSheetAsync(int userId, int sheetMusicId, bool isFavorite = true)
        {
            return await _userFavoriteSheetService.UpdateUserFavoriteSheetAsync(userId, sheetMusicId, isFavorite);
        }

        [HttpDelete("{userId}/{sheetMusicId}")]
        public async Task<bool> DeleteUserFavoriteSheetAsync([FromQuery] int userId,[FromQuery]  int sheetMusicId)
        {
            return await _userFavoriteSheetService.DeleteUserFavoriteSheetAsync(userId, sheetMusicId);
        }
    }
}
