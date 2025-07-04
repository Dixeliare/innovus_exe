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
    public class OpeningScheduleController : ControllerBase
    {
        private readonly IOpeningScheduleService _openingScheduleService;
        
        public OpeningScheduleController(IOpeningScheduleService openingScheduleService) => _openingScheduleService = openingScheduleService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OpeningScheduleDto>>> GetAllAsync()
        {
            var schedules = await _openingScheduleService.GetAllAsync();
            return Ok(schedules);
        }

        [HttpGet("search_by")]
        public async Task<ActionResult<IEnumerable<OpeningScheduleDto>>> SearchByAsync([FromQuery] string? subject = null,
            [FromQuery] string? classCode = null, [FromQuery] DateOnly? openingDay = null,
            [FromQuery] DateOnly? endDate = null, [FromQuery] string? schedule = null,
            [FromQuery] int? studentQuantity = null, [FromQuery] bool? isAdvancedClass = null)
        {
            var schedules = await _openingScheduleService.SearchOpeningSchedulesAsync(subject, classCode, openingDay, endDate, schedule, studentQuantity, isAdvancedClass);
            return Ok(schedules); // Service đã trả về DTO
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OpeningScheduleDto>> GetOpeningScheduleById(int id)
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var schedule = await _openingScheduleService.GetByIdAsync(id);
            return Ok(schedule); // Service đã trả về DTO
        }

        // POST: api/OpeningSchedules
        [HttpPost]
        public async Task<ActionResult<OpeningScheduleDto>> CreateOpeningSchedule([FromBody] CreateOpeningScheduleDto createOpeningScheduleDto)
        {
            var createdSchedule = await _openingScheduleService.AddAsync(createOpeningScheduleDto);
            return CreatedAtAction(nameof(GetOpeningScheduleById), new { id = createdSchedule.OpeningScheduleId }, createdSchedule);
        }

        // PUT: api/OpeningSchedules/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOpeningSchedule(int id, [FromBody] UpdateOpeningScheduleDto updateOpeningScheduleDto)
        {
            if (id != updateOpeningScheduleDto.OpeningScheduleId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "OpeningScheduleId", new string[] { "ID lịch khai giảng trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            await _openingScheduleService.UpdateAsync(updateOpeningScheduleDto);
            return NoContent();
        }

        // DELETE: api/OpeningSchedules/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOpeningSchedule(int id)
        {
            
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ApiException nếu có lỗi.
            await _openingScheduleService.DeleteAsync(id);
            return NoContent();
        }
    }
}
