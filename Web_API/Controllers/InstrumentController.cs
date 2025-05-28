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
    public class InstrumentController : ControllerBase
    {
        private readonly IInstrumentService _instrumentService;
        
        public InstrumentController(IInstrumentService instrumentService) => _instrumentService = instrumentService;

        [HttpGet("search_by_instrument_name")]
        public async Task<IEnumerable<instrument>> SearchInstrumentsAsync([FromQuery] string? instrumentName = null)
        {
            return await _instrumentService.SearchInstrumentsAsync(instrumentName);
        }

        [HttpGet]
        public async Task<IEnumerable<instrument>> GetAllAsync()
        {
            return await _instrumentService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<instrument> GetAsync(int id)
        {
            return await _instrumentService.GetByIdAsync(id);
        }

        [HttpPost]
        public async Task<int> AddAsync(instrument instrument)
        {
            return await _instrumentService.CreateAsync(instrument);
        }

        [HttpPut]
        public async Task<int> UpdateAsync(instrument instrument)
        {
            return await _instrumentService.UpdateAsync(instrument);
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(int id)
        {
            return await _instrumentService.DeleteAsync(id);
        }
    }
}
