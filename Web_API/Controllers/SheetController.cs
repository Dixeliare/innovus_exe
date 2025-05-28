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
    public class SheetController : ControllerBase
    {
        private readonly ISheetService _sheetService;
        
        public SheetController(ISheetService sheetService) => _sheetService = sheetService;

        [HttpGet]
        public async Task<IEnumerable<sheet>> GetAllAsync()
        {
            return await _sheetService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<sheet> CreateAsync(int id)
        {
            return await _sheetService.GetByIdAsync(id);
        }

        [HttpPut]
        public async Task<int> UpdateAsync(sheet sheet)
        {
            return await _sheetService.UpdateAsync(sheet);
        }

        [HttpPost]
        public async Task<int> CreateAsync(sheet sheet)
        {
            return await _sheetService.CreateAsync(sheet);
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(int id)
        {
            return await _sheetService.DeleteAsync(id);
        }
    }
}
