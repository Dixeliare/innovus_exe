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
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        
        public ClassController(IClassService classService) => _classService = classService;

        [HttpGet]
        public async Task<IEnumerable<_class>> GetAll()
        {
            return await _classService.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<_class> GetById(int id)
        {
            return await _classService.GetById(id);
        }

        [HttpPost]
        public async Task<int> CreateAsync(_class entity)
        {
            return await _classService.CreateAsync(entity);
        }

        [HttpPut]
        public async Task<int> UpdateAsync(_class entity)
        {
            return await _classService.UpdateAsync(entity);
        }

        [HttpDelete]
        public async Task<bool> DeleteAsync(int id)
        {
            return await _classService.DeleteAsync(id);
        }

        [HttpGet("search_by_instrumet_id_or_class_code")]
        public async Task<IEnumerable<_class>> SearchAsync(int? instrumentId = null, string? classCode = null)
        {
            return await _classService.SearchClassesAsync(instrumentId, classCode);
        }
    }
}
