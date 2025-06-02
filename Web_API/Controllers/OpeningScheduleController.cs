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
    public class OpeningScheduleController : ControllerBase
    {
        private readonly IOpeningScheduleService _openingScheduleService;
        
        public OpeningScheduleController(IOpeningScheduleService openingScheduleService) => _openingScheduleService = openingScheduleService;

        [HttpGet]
        public async Task<IEnumerable<opening_schedule>> GetAllAsync()
        {
            return await _openingScheduleService.GetAllAsync();
        }

        [HttpGet("search_by")]
        public async Task<IEnumerable<opening_schedule>> SearchByAsync([FromQuery] string? subject = null,
            [FromQuery] string? classCode = null, [FromQuery] DateOnly? openingDay = null,
            [FromQuery] DateOnly? endDate = null, [FromQuery] string? schedule = null,
            [FromQuery] int? studentQuantity = null, [FromQuery] bool? isAdvancedClass = null)
        {
            return await _openingScheduleService.SearchOpeningSchedulesAsync(subject, classCode, openingDay, endDate, schedule, studentQuantity, isAdvancedClass);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OpeningScheduleDto>> GetOpeningScheduleById(int id)
        {
            var schedule = await _openingScheduleService.GetByIdAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }
            return Ok(schedule);
        }

        // POST: api/OpeningSchedules
        [HttpPost]
        public async Task<ActionResult<OpeningScheduleDto>> CreateOpeningSchedule([FromBody] CreateOpeningScheduleDto createOpeningScheduleDto)
        {
            try
            {
                var createdSchedule = await _openingScheduleService.AddAsync(createOpeningScheduleDto);
                return CreatedAtAction(nameof(GetOpeningScheduleById), new { id = createdSchedule.OpeningScheduleId }, createdSchedule);
            }
            catch (Exception ex)
            {
                // Vì không có khóa ngoại, lỗi thường là do validation hoặc DB
                return StatusCode(500, new { message = "An error occurred while creating the opening schedule.", error = ex.Message });
            }
        }

        // PUT: api/OpeningSchedules/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOpeningSchedule(int id, [FromBody] UpdateOpeningScheduleDto updateOpeningScheduleDto)
        {
            if (id != updateOpeningScheduleDto.OpeningScheduleId)
            {
                return BadRequest(new { message = "Opening Schedule ID in URL does not match ID in body." });
            }

            try
            {
                await _openingScheduleService.UpdateAsync(updateOpeningScheduleDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the opening schedule.", error = ex.Message });
            }
        }

        // DELETE: api/OpeningSchedules/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOpeningSchedule(int id)
        {
            try
            {
                await _openingScheduleService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the opening schedule.", error = ex.Message });
            }
        }
    }
}
