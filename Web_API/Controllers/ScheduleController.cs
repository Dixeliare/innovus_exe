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
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService? _scheduleService;
        
        public ScheduleController(IScheduleService? scheduleService) => _scheduleService = scheduleService;

        [HttpGet]
        public async Task<IEnumerable<schedule>> GetAll()
        {
            return await _scheduleService.GetAllAsync();
        }
        [HttpGet("search_id_or_note")]
        public async Task<IEnumerable<schedule>> SearchByIdOrNote([FromQuery] int? id,[FromQuery] string? note)
        {
            return await _scheduleService.SearchByIdOrNoteAsync(id, note);
        }

        [HttpGet("search_month_or_year")]
        public async Task<IEnumerable<schedule>> SearchByMonthYearAsync([FromQuery]int month,[FromQuery] int year)
        {
            return await _scheduleService.SearchByMonthYearAsync(month, year);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleDto>> GetScheduleById(int id)
        {
            var schedule = await _scheduleService.GetByIDAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }
            return Ok(schedule);
        }

        // POST: api/Schedules
        [HttpPost]
        public async Task<ActionResult<ScheduleDto>> CreateSchedule([FromBody] CreateScheduleDto createScheduleDto)
        {
            try
            {
                var createdSchedule = await _scheduleService.AddAsync(createScheduleDto);
                return CreatedAtAction(nameof(GetScheduleById), new { id = createdSchedule.ScheduleId }, createdSchedule);
            }
            catch (KeyNotFoundException ex) // Giữ lại để bắt các lỗi khác nếu có
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the schedule.", error = ex.Message });
            }
        }

        // PUT: api/Schedules/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleDto updateScheduleDto)
        {
            if (id != updateScheduleDto.ScheduleId)
            {
                return BadRequest(new { message = "Schedule ID in URL does not match ID in body." });
            }

            try
            {
                await _scheduleService.UpdateAsync(updateScheduleDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the schedule.", error = ex.Message });
            }
        }

        // DELETE: api/Schedules/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                await _scheduleService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the schedule.", error = ex.Message });
            }
        }
    }
}
