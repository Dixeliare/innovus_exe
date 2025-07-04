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
    public class InstrumentController : ControllerBase
    {
        private readonly IInstrumentService _instrumentService;
        
        public InstrumentController(IInstrumentService instrumentService) => _instrumentService = instrumentService;

        [HttpGet("search_by_instrument_name")]
        public async Task<ActionResult<IEnumerable<instrument>>> SearchInstrumentsAsync([FromQuery] string? instrumentName = null)
        {
            var instruments = await _instrumentService.SearchInstrumentsAsync(instrumentName);
            return Ok(instruments); // Service already returns DTOs
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<instrument>>> GetAllAsync()
        {
            var instruments = await _instrumentService.GetAllAsync();
            return Ok(instruments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InstrumentDto>> GetInstrumentById(int id)
        {
            var instrument = await _instrumentService.GetByIdAsync(id);
            return Ok(instrument); // Service already returns DTO

        }

        // POST: api/Instruments
        [HttpPost]
        public async Task<ActionResult<InstrumentDto>> CreateInstrument([FromBody] CreateInstrumentDto createInstrumentDto)
        {
            var createdInstrument = await _instrumentService.AddAsync(createInstrumentDto);
            return CreatedAtAction(nameof(GetInstrumentById), new { id = createdInstrument.InstrumentId }, createdInstrument);
        }

        // PUT: api/Instruments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInstrument(int id, [FromBody] UpdateInstrumentDto updateInstrumentDto)
        {
            if (id != updateInstrumentDto.InstrumentId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "InstrumentId", new string[] { "ID nhạc cụ trong URL không khớp với ID trong body." } }
                });
            }

            // No try-catch here. Service will throw NotFoundException/ValidationException/ApiException if there's an error.
            await _instrumentService.UpdateAsync(updateInstrumentDto);
            return NoContent();
        }

        // DELETE: api/Instruments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstrument(int id)
        {
            await _instrumentService.DeleteAsync(id);
            return NoContent();
        }
    }
}
