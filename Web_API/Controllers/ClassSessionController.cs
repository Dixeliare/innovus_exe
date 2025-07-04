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
    public class ClassSessionController : ControllerBase
    {
        private readonly IClassSessionService _classSessionService;
        
        public ClassSessionController(IClassSessionService classSessionService) => _classSessionService = classSessionService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassSessionDto>>> GetAllAsync() // Trả về DTOs
        {
            var classSessions = await _classSessionService.GetAllAsync();
            return Ok(classSessions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClassSessionDto>> GetClassSessionById(int id) // Trả về DTO
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var classSession = await _classSessionService.GetByIdAsync(id);
            return Ok(classSession); // Service đã trả về DTO
        }

        [HttpGet("search_by")] // Đổi tên đường dẫn cho rõ ràng hơn
        public async Task<ActionResult<IEnumerable<ClassSessionDto>>> SearchClassSessions( // Trả về DTOs
            [FromQuery] DateOnly? date = null,
            [FromQuery] string? roomCode = null,
            [FromQuery] int? weekId = null,
            [FromQuery] int? classId = null,
            [FromQuery] int? timeSlotId = null)
        {
            var classSessions = await _classSessionService.SearchClassSessionsAsync(date, roomCode, weekId, classId, timeSlotId);
            return Ok(classSessions); // Service đã trả về DTOs
        }

        [HttpPost]
        public async Task<ActionResult<ClassSessionDto>> CreateClassSession([FromBody] CreateClassSessionDto createClassSessionDto)
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            var createdClassSession = await _classSessionService.AddAsync(createClassSessionDto);
            return CreatedAtAction(nameof(GetClassSessionById), new { id = createdClassSession.ClassSessionId }, createdClassSession);
        }

        // PUT: api/ClassSessions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClassSession(int id, [FromBody] UpdateClassSessionDto updateClassSessionDto)
        {
            if (id != updateClassSessionDto.ClassSessionId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ClassSessionId", new string[] { "ID buổi học trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            await _classSessionService.UpdateAsync(updateClassSessionDto);
            return NoContent();
        }

        [HttpDelete("{id}")] // Xóa theo ID từ URL
        public async Task<IActionResult> DeleteClassSession(int id) // Trả về IActionResult
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ApiException nếu có lỗi.
            await _classSessionService.DeleteAsync(id);
            return NoContent();
        }
    }
}
