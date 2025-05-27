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
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;
        
        public GenreController(IGenreService genreService) => _genreService = genreService;

        [HttpGet("search_by_genre_name")]
        public async Task<IEnumerable<genre>> SearchGenresAsync([FromQuery ]string? genreName = null)
        {
            return await _genreService.SearchGenresAsync(genreName);
        }

        [HttpGet]
        public async Task<IEnumerable<genre>> GetAll()
        {
            return await _genreService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<genre> GetById(int id)
        {
            return await _genreService.GetByIdAsync(id);
        }

        [HttpPost]
        public async Task<int> Post([FromBody] genre genre)
        {
            return await _genreService.CreateAsync(genre);
        }

        [HttpPut]
        public async Task<int> Put([FromBody] genre genre)
        {
            return await _genreService.UpdateAsync(genre);
        }

        [HttpDelete]
        public async Task<bool> Delete([FromBody] int id)
        {
            return await _genreService.DeleteAsync(id);
        }
    }
}
