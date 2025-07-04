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
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService? _scheduleService;
        
        public ScheduleController(IScheduleService? scheduleService) => _scheduleService = scheduleService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetAllAsync() // Trả về ScheduleDto
        {
            var schedules = await _scheduleService.GetAllAsync();
            return Ok(schedules); // Service đã trả về DTO
        }

        [HttpGet("search_id_or_note")]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> SearchByIdOrNote([FromQuery] int? id, [FromQuery] string? note)
        {
            var schedules = await _scheduleService.SearchByIdOrNoteAsync(id, note);
            return Ok(schedules); // Service đã trả về DTO
        }

        [HttpGet("search_month_or_year")]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> SearchByMonthYearAsync([FromQuery]int month, [FromQuery] int year)
        {
            var schedules = await _scheduleService.SearchByMonthYearAsync(month, year);
            return Ok(schedules); // Service đã trả về DTO
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleDto>> GetScheduleById(int id)
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var schedule = await _scheduleService.GetByIDAsync(id);
            return Ok(schedule); // Service đã trả về DTO
        }

        // POST: api/Schedules
        [HttpPost]
        public async Task<ActionResult<ScheduleDto>> CreateSchedule([FromBody] CreateScheduleDto createScheduleDto)
        {
            // Không có try-catch ở đây
            var createdSchedule = await _scheduleService.AddAsync(createScheduleDto);
            return CreatedAtAction(nameof(GetScheduleById), new { id = createdSchedule.ScheduleId }, createdSchedule);
        }

        // PUT: api/Schedules/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleDto updateScheduleDto)
        {
            if (id != updateScheduleDto.ScheduleId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ScheduleId", new string[] { "ID lịch trình trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây
            await _scheduleService.UpdateAsync(updateScheduleDto);
            return NoContent();
        }

        // DELETE: api/Schedules/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            // Không có try-catch ở đây
            await _scheduleService.DeleteAsync(id);
            return NoContent();
        }
    }
}
