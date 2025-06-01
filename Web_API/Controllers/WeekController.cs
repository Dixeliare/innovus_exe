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
    public class WeekController : ControllerBase
    {
        private readonly IWeekService _weekService;
        
        public WeekController(IWeekService weekService) => _weekService = weekService;
        
        [HttpGet]
        public async Task<IEnumerable<week>> GetAll()
        {
            return await _weekService.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<week> GetById(int id)
        {
            return await _weekService.GetById(id);
        }

        [HttpGet("BySchedule/{scheduleId}")]
        public async Task<ActionResult<IEnumerable<WeekDto>>> GetWeeksByScheduleId(int scheduleId)
        {
            var weeks = await _weekService.GetWeeksByScheduleIdAsync(scheduleId);
            if (!weeks.Any())
            {
                // Có thể trả về 200 OK với danh sách rỗng hoặc 404 nếu bạn muốn chỉ khi không có lịch trình nào
                return NotFound($"No weeks found for schedule ID {scheduleId}.");
            }
            return Ok(weeks);
        }

        // POST: api/Weeks
        [HttpPost]
        public async Task<ActionResult<WeekDto>> CreateWeek([FromBody] CreateWeekDto createWeekDto)
        {
            try
            {
                var createdWeek = await _weekService.AddAsync(createWeekDto);
                return CreatedAtAction(nameof(GetById), new { id = createdWeek.WeekId }, createdWeek);
            }
            catch (KeyNotFoundException ex) // Bắt lỗi khóa ngoại
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the week.", error = ex.Message });
            }
        }

        // PUT: api/Weeks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWeek(int id, [FromBody] UpdateWeekDto updateWeekDto)
        {
            if (id != updateWeekDto.WeekId)
            {
                return BadRequest(new { message = "Week ID in URL does not match ID in body." });
            }

            try
            {
                await _weekService.UpdateAsync(updateWeekDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the week.", error = ex.Message });
            }
        }

        [HttpGet("search_by_day_of_week_or_schedule_id")]
        public async Task<IEnumerable<week>> SearchByDayOfWeekOrScheduleId([FromQuery] DateOnly? dayOfWeek,[FromQuery] int? scheduleId)
        {
            return await _weekService.SearchWeeksAsync(dayOfWeek, scheduleId);
        }

        [HttpDelete("{id}")]
        public async Task<bool> Delete([FromBody] int id)
        {
            return await _weekService.DeleteAsync(id);
        }
    }
}