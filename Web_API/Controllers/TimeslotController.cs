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
    public class TimeslotController : ControllerBase
    {
        private readonly ITimeslotService _timeslotService;
        
        public TimeslotController(ITimeslotService timeslotService) => _timeslotService = timeslotService;

        [HttpGet]
        public async Task<IEnumerable<timeslot>> GetAll()
        {
            return await _timeslotService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TimeslotDto>> GetTimeslotById(int id)
        {
            var timeslot = await _timeslotService.GetByIDAsync(id);
            if (timeslot == null)
            {
                return NotFound();
            }
            return Ok(timeslot);
        }

        // POST: api/Timeslots
        [HttpPost]
        public async Task<ActionResult<TimeslotDto>> CreateTimeslot([FromBody] CreateTimeslotDto createTimeslotDto)
        {
            try
            {
                var createdTimeslot = await _timeslotService.AddAsync(createTimeslotDto);
                return CreatedAtAction(nameof(GetTimeslotById), new { id = createdTimeslot.TimeslotId }, createdTimeslot);
            }
            catch (ArgumentException ex) // Bắt lỗi validation thời gian
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the timeslot.", error = ex.Message });
            }
        }

        // PUT: api/Timeslots/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimeslot(int id, [FromBody] UpdateTimeslotDto updateTimeslotDto)
        {
            if (id != updateTimeslotDto.TimeslotId)
            {
                return BadRequest(new { message = "Timeslot ID in URL does not match ID in body." });
            }

            try
            {
                await _timeslotService.UpdateAsync(updateTimeslotDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex) // Bắt lỗi validation thời gian
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the timeslot.", error = ex.Message });
            }
        }

        // DELETE: api/Timeslots/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeslot(int id)
        {
            try
            {
                await _timeslotService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the timeslot.", error = ex.Message });
            }
        }
    }
}
