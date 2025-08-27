using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Authorization;
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
    [Produces("application/json")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService) => _attendanceService = attendanceService;

        /// <summary>
        /// Lấy tất cả các bản điểm danh.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AttendanceDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAllAsync()
        {
            try
            {
                var attendances = await _attendanceService.GetAllAsync();
                return Ok(attendances);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "An error occurred while retrieving attendances.", details = ex.Message });
            }
        }

        /// <summary>
        /// Lấy bản điểm danh theo ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AttendanceDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<AttendanceDto>> GetById(int id)
        {
            try
            {
                var attendance = await _attendanceService.GetByIdAsync(id);
                return Ok(attendance);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new
                    {
                        message = $"An error occurred while retrieving attendance with ID {id}.", details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Lấy danh sách điểm danh theo User ID.
        /// </summary>
        [HttpGet("byUser/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<AttendanceDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendancesByUserId(int userId)
        {
            try
            {
                var attendances = await _attendanceService.GetAttendancesByUserIdAsync(userId);
                return Ok(attendances);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new
                    {
                        message = $"An error occurred while retrieving attendances for user ID {userId}.",
                        details = ex.Message
                    });
            }
        }

        /// <summary>
        /// Lấy danh sách điểm danh theo Class Session ID.
        /// </summary>
        [HttpGet("byClassSession/{classSessionId}")]
        [ProducesResponseType(typeof(IEnumerable<AttendanceDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendancesByClassSessionId(int classSessionId)
        {
            try
            {
                var attendances = await _attendanceService.GetAttendancesByClassSessionIdAsync(classSessionId);
                return Ok(attendances);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new
                    {
                        message =
                            $"An error occurred while retrieving attendances for class session ID {classSessionId}.",
                        details = ex.Message
                    });
            }
        }


        /// <summary>
        /// Tìm kiếm bản điểm danh theo trạng thái, ghi chú, User ID hoặc Class Session ID.
        /// </summary>
        [HttpGet("search")] // Đổi tên đường dẫn cho rõ ràng hơn
        [ProducesResponseType(typeof(IEnumerable<AttendanceDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> SearchAttendancesAsync(
            // ĐÃ SỬA: Thay đổi kiểu của tham số 'status' từ 'bool?' sang 'int?'
            // Đồng thời đổi tên thành statusId để nhất quán với service/repository
            [FromQuery] int? statusId = null,
            [FromQuery] string? note = null,
            [FromQuery] int? userId = null,
            [FromQuery] int? classSessionId = null)
        {
            try
            {
                // ĐÃ SỬA: Truyền statusId vào phương thức service
                var attendances =
                    await _attendanceService.SearchAttendancesAsync(statusId, note, userId, classSessionId);
                return Ok(attendances);
            }
            catch (Exception ex)
            {
                // Logging the exception is recommended here
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "An error occurred during attendance search.", details = ex.Message });
            }
        }

        /// <summary>
        /// Thêm bản điểm danh mới.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AttendanceDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<AttendanceDto>> Add([FromBody] CreateAttendanceDto createAttendanceDto)
        {
            try
            {
                var newAttendance = await _attendanceService.AddAsync(createAttendanceDto);
                return CreatedAtAction(nameof(GetById), new { id = newAttendance.AttendanceId }, newAttendance);
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
        /// Cập nhật bản điểm danh.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAttendanceDto updateAttendanceDto)
        {
            if (id != updateAttendanceDto.AttendanceId)
            {
                return BadRequest(new { message = "ID điểm danh trong URL không khớp với ID trong body." });
            }

            try
            {
                await _attendanceService.UpdateAsync(updateAttendanceDto);
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
        /// Xóa bản điểm danh theo ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _attendanceService.DeleteAsync(id);
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
        /// Bulk update attendance records.
        /// </summary>
        [HttpPut("bulk")]
        public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateAttendanceDto bulkUpdateDto)
        {
            try
            {
                await _attendanceService.BulkUpdateAsync(bulkUpdateDto);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = "Validation error", errors = ex.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An error occurred while bulk updating attendance.", details = ex.Message });
            }
        }
    }
}