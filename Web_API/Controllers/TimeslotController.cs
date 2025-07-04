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
    public class TimeslotController : ControllerBase
    {
        private readonly ITimeslotService _timeslotService;
        
        public TimeslotController(ITimeslotService timeslotService) => _timeslotService = timeslotService;

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var timeslots = await _timeslotService.GetAllAsync();
            return Ok(timeslots);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TimeslotDto>> GetTimeslotById(int id)
        {
            var timeslot = await _timeslotService.GetByIDAsync(id);
            return Ok(timeslot);
        }
        
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<timeslot>>> SearchByStartTimeOrEndTime(
            [FromQuery] TimeOnly? startTime,
            [FromQuery] TimeOnly? endTime)
        {
            var timeslots = await _timeslotService.SearchByStartTimeOrEndTimeAsync(startTime, endTime);
            return Ok(timeslots);
        }

        // POST: api/Timeslots
        [HttpPost]
        public async Task<ActionResult<TimeslotDto>> CreateTimeslot([FromBody] CreateTimeslotDto createTimeslotDto)
        {
            var createdTimeslot = await _timeslotService.AddAsync(createTimeslotDto);
            return CreatedAtAction(nameof(GetTimeslotById), new { id = createdTimeslot.TimeslotId }, createdTimeslot);
        }

        // PUT: api/Timeslots/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimeslot(int id, [FromBody] UpdateTimeslotDto updateTimeslotDto)
        {
            if (id != updateTimeslotDto.TimeslotId)
            {
                // Ném ValidationException thay vì BadRequest
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "TimeslotId", new string[] { "ID khung thời gian trong URL không khớp với ID trong body." } }
                });
            }

            // Không còn try-catch ở đây, Service sẽ ném NotFoundException/ValidationException/ApiException
            await _timeslotService.UpdateAsync(updateTimeslotDto);
            return NoContent(); // 204 No Content for successful update
        }

        // DELETE: api/Timeslots/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeslot(int id)
        {
            // Không còn try-catch ở đây, Service sẽ ném NotFoundException/ApiException
            await _timeslotService.DeleteAsync(id);
            return NoContent(); // 204 No Content for successful deletion
        }
    }
}
