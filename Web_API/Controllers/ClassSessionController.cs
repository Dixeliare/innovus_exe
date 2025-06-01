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
    public class ClassSessionController : ControllerBase
    {
        private readonly IClassSessionService _classSessionService;
        
        public ClassSessionController(IClassSessionService classSessionService) => _classSessionService = classSessionService;

        [HttpGet]
        public async Task<IEnumerable<class_session>> GetAllAsync()
        {
            return await _classSessionService.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClassSessionDto>> GetClassSessionById(int id)
        {
            var classSession = await _classSessionService.GetByIdAsync(id);
            if (classSession == null)
            {
                return NotFound();
            }
            return Ok(classSession);
        }

        [HttpGet("search_by_date_or_room_code_or_week_id_or_class_id_or_time_slot_id")]
        public async Task<IEnumerable<class_session>> SearchClassSessions([FromQuery] DateOnly? date = null,[FromQuery] string? roomCode = null,[FromQuery] int? weekId = null,[FromQuery] int? classId = null,
            [FromQuery] int? timeSlotId = null)
        {
            return await _classSessionService.SearchClassSessionsAsync(date, roomCode, weekId, classId, timeSlotId);
        }

        [HttpPost]
        public async Task<ActionResult<ClassSessionDto>> CreateClassSession([FromBody] CreateClassSessionDto createClassSessionDto)
        {
            try
            {
                var createdClassSession = await _classSessionService.AddAsync(createClassSessionDto);
                return CreatedAtAction(nameof(GetClassSessionById), new { id = createdClassSession.ClassSessionId }, createdClassSession);
            }
            catch (KeyNotFoundException ex)
            {
                // Xử lý lỗi khóa ngoại nếu ID không tồn tại
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                return StatusCode(500, new { message = "An error occurred while creating the class session.", error = ex.Message });
            }
        }

        // PUT: api/ClassSessions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClassSession(int id, [FromBody] UpdateClassSessionDto updateClassSessionDto)
        {
            if (id != updateClassSessionDto.ClassSessionId)
            {
                return BadRequest(new { message = "Class Session ID in URL does not match ID in body." });
            }

            try
            {
                await _classSessionService.UpdateAsync(updateClassSessionDto);
                return NoContent(); // 204 No Content for successful update
            }
            catch (KeyNotFoundException ex)
            {
                // Xử lý lỗi nếu bản ghi không tồn tại hoặc khóa ngoại không hợp lệ
                return NotFound(new { message = ex.Message }); // Hoặc BadRequest nếu lỗi là do FK không hợp lệ
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the class session.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync([FromBody] int value)
        {
            return await _classSessionService.DeleteAsync(value);
        }
    }
}
