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
        public async Task<ActionResult<InstrumentDto>> GetInstrumentById(int id)
        {
            var instrument = await _instrumentService.GetByIdAsync(id);
            if (instrument == null)
            {
                return NotFound();
            }
            return Ok(instrument);
        }

        // POST: api/Instruments
        [HttpPost]
        public async Task<ActionResult<InstrumentDto>> CreateInstrument([FromBody] CreateInstrumentDto createInstrumentDto)
        {
            try
            {
                var createdInstrument = await _instrumentService.AddAsync(createInstrumentDto);
                return CreatedAtAction(nameof(GetInstrumentById), new { id = createdInstrument.InstrumentId }, createdInstrument);
            }
            catch (Exception ex)
            {
                // Vì không có khóa ngoại, lỗi thường là do validation hoặc DB
                return StatusCode(500, new { message = "An error occurred while creating the instrument.", error = ex.Message });
            }
        }

        // PUT: api/Instruments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInstrument(int id, [FromBody] UpdateInstrumentDto updateInstrumentDto)
        {
            if (id != updateInstrumentDto.InstrumentId)
            {
                return BadRequest(new { message = "Instrument ID in URL does not match ID in body." });
            }

            try
            {
                await _instrumentService.UpdateAsync(updateInstrumentDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the instrument.", error = ex.Message });
            }
        }

        // DELETE: api/Instruments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstrument(int id)
        {
            try
            {
                await _instrumentService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the instrument.", error = ex.Message });
            }
        }
    }
}
