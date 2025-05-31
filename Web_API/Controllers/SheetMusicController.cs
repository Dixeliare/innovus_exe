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
    public class SheetMusicController : ControllerBase
    {
        private readonly ISheetMusicService _sheetMusicService;
        
        public SheetMusicController(ISheetMusicService sheetMusicService) => _sheetMusicService = sheetMusicService;

        [HttpGet]
        public async Task<IEnumerable<sheet_music>> GetAllAsync()
        {
            return await _sheetMusicService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<sheet_music> GetByIdAsync(int id)
        {
            return await _sheetMusicService.GetByIdAsync(id);
        }

        [HttpGet("search_by")]
        public async Task<IEnumerable<sheet_music>> SeachBySheetMusicAsync(
            int? number = null,
            string? musicName = null,
            string? composer = null,
            int? sheetQuantity = null,
            int? favoriteCount = null)
        {
            return await _sheetMusicService.SearchSheetMusicAsync(number, musicName, composer, sheetQuantity,
                favoriteCount);
        }

        [HttpPost]
        public async Task<int> PostAsync([FromBody] sheet_music value)
        {
            return await _sheetMusicService.CreateAsync(value);
        }

        [HttpPut]
        public async Task<int> PutAsync([FromBody] sheet_music value)
        {
            return await _sheetMusicService.UpdateAsync(value);
        }

        [HttpDelete]
        public async Task<bool> DeleteAsync([FromBody] int id)
        {
            return await _sheetMusicService.DeleteAsync(id);
        }
    }
}
