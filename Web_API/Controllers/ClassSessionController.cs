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
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    //[Authorize] // Áp dụng cho toàn bộ controller
    public class ClassSessionController : ControllerBase
    {
        private readonly IClassSessionService _classSessionService;

        public ClassSessionController(IClassSessionService classSessionService)
        {
            _classSessionService = classSessionService;
        }

        /// <summary>
        /// Lấy tất cả các buổi học với thông tin chi tiết.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PersonalClassSessionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllClassSessions()
        {
            try
            {
                var sessions = await _classSessionService.GetAllAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "An error occurred while retrieving class sessions.", details = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin buổi học chi tiết theo ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PersonalClassSessionDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClassSessionById(int id)
        {
            try
            {
                var session = await _classSessionService.GetByIdAsync(id);
                return Ok(session);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy danh sách buổi học chi tiết theo ID lớp.
        /// </summary>
        [HttpGet("byClass/{classId}")]
        [ProducesResponseType(typeof(IEnumerable<PersonalClassSessionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClassSessionsByClassId(int classId)
        {
            try
            {
                var sessions = await _classSessionService.GetClassSessionsByClassIdAsync(classId);
                return Ok(sessions);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy danh sách buổi học chi tiết theo ID ngày.
        /// </summary>
        [HttpGet("byDay/{dayId}")]
        [ProducesResponseType(typeof(IEnumerable<PersonalClassSessionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetClassSessionsByDayId(int dayId)
        {
            try
            {
                var sessions = await _classSessionService.GetClassSessionsByDayIdAsync(dayId);
                return Ok(sessions);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Tạo buổi học mới.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseClassSessionDto),
            (int)HttpStatusCode.Created)] // Changed to BaseClassSessionDto
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateClassSession([FromBody] CreateClassSessionDto createClassSessionDto)
        {
            try
            {
                var newSession = await _classSessionService.AddAsync(createClassSessionDto);
                // After creation, you might want to fetch the full PersonalClassSessionDto
                // return CreatedAtAction(nameof(GetClassSessionById), new { id = newSession.ClassSessionId }, newSession);
                // Or just return the BaseClassSessionDto if it's sufficient for create response
                return StatusCode((int)HttpStatusCode.Created, newSession);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ApiException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin buổi học.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateClassSession(int id,
            [FromBody] UpdateClassSessionDto updateClassSessionDto)
        {
            if (id != updateClassSessionDto.ClassSessionId)
            {
                return BadRequest(new { message = "ID trong URL và ID trong request body không khớp." });
            }

            try
            {
                await _classSessionService.UpdateAsync(updateClassSessionDto);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ApiException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Xóa buổi học theo ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteClassSession(int id)
        {
            try
            {
                var result = await _classSessionService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Class Session with ID {id} not found." });
                }

                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ApiException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Tìm kiếm buổi học theo các tiêu chí.
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<PersonalClassSessionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SearchClassSessions(
            [FromQuery] int? sessionNumber = null,
            [FromQuery] DateOnly? date = null,
            [FromQuery] int? roomId = null, // ĐÃ SỬA: Thay đổi từ string? roomCode sang int? roomId
            [FromQuery] int? classId = null,
            [FromQuery] int? dayId = null,
            [FromQuery] int? timeSlotId = null)
        {
            try
            {
                var sessions = await _classSessionService.SearchClassSessionsAsync(
                    sessionNumber, date, roomId, classId, dayId, timeSlotId // ĐÃ SỬA: Truyền roomId
                );
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = $"An error occurred during search: {ex.Message}" });
            }
        }

        [HttpGet("{id}/users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersInClassSession(int id)
        {
            try
            {
                var users = await _classSessionService.GetUsersInClassSessionAsync(id);
                return Ok(users);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new
                    {
                        message = "An error occurred while retrieving users in class session.", details = ex.Message
                    });
            }
        }
    }
}