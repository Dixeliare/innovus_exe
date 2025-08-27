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
using System.Net; // Thêm namespace này cho HttpStatusCode
using Microsoft.AspNetCore.Authorization; // Thêm cho Authorize

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService; // Removed '?' to enforce non-null

        public ScheduleController(IScheduleService scheduleService) => _scheduleService = scheduleService;

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ScheduleDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetAllAsync()
        {
            var schedules = await _scheduleService.GetAllAsync();
            return Ok(schedules);
        }

        [HttpGet("search_id_or_note")]
        [ProducesResponseType(typeof(IEnumerable<ScheduleDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> SearchByIdOrNote([FromQuery] int? id, [FromQuery] string? note)
        {
            var schedules = await _scheduleService.SearchByIdOrNoteAsync(id, note);
            return Ok(schedules);
        }

        [HttpGet("search_month_or_year")]
        [ProducesResponseType(typeof(IEnumerable<ScheduleDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> SearchByMonthYearAsync([FromQuery]int month, [FromQuery] int year)
        {
            // DÒNG BỊ LỖI ĐƯỢC SỬA Ở ĐÂY:
            var schedules = await _scheduleService.GetSchedulesInMonthYearAsync(month, year); // <--- Đổi tên hàm
            return Ok(schedules);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ScheduleDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<ScheduleDto>> GetScheduleById(int id)
        {
            var schedule = await _scheduleService.GetByIDAsync(id);
            return Ok(schedule);
        }

        // POST: api/Schedules
        // While schedules are mostly auto-generated, keep this for manual creation/testing.
        [HttpPost]
        [ProducesResponseType(typeof(ScheduleDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)] // If MonthYear already exists
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<ScheduleDto>> CreateSchedule([FromBody] CreateScheduleDto createScheduleDto)
        {
            var createdSchedule = await _scheduleService.AddAsync(createScheduleDto);
            return CreatedAtAction(nameof(GetScheduleById), new { id = createdSchedule.ScheduleId }, createdSchedule);
        }

        // PUT: api/Schedules/{id}
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)] // For ID mismatch or validation errors
        [ProducesResponseType((int)HttpStatusCode.Conflict)] // If MonthYear update causes a conflict
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleDto updateScheduleDto)
        {
            if (id != updateScheduleDto.ScheduleId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ScheduleId", new string[] { "ID lịch trình trong URL không khớp với ID trong body." } }
                });
            }

            await _scheduleService.UpdateAsync(updateScheduleDto);
            return NoContent();
        }

        // DELETE: api/Schedules/{id}
        // While schedules are mostly auto-managed, keep this for manual deletion/testing.
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)] // If there are related entities preventing deletion
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        // [Authorize(Roles = "1,2")] // Example: Only Admin, Manager can delete schedules
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            await _scheduleService.DeleteAsync(id);
            return NoContent();
        }
    }
}