using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;
using Services.IServices;
using Services.Exceptions;
using ValidationException = Services.Exceptions.ValidationException;
using System.Net;
using Microsoft.AspNetCore.Authorization; // Thêm namespace này cho HttpStatusCode

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    //[Authorize] // Thêm Authorize nếu bạn muốn bảo vệ endpoint này
    public class WeekController : ControllerBase
    {
        private readonly IWeekService _weekService;
        
        public WeekController(IWeekService weekService) => _weekService = weekService;
        
        /// <summary>
        /// Lấy tất cả các tuần.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<WeekDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)] // Thêm loại phản hồi lỗi
        public async Task<ActionResult<IEnumerable<WeekDto>>> GetAll()
        {
            try
            {
                var weeks = await _weekService.GetAllAsync();
                return Ok(weeks);
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây nếu bạn có logger
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred while retrieving all weeks.", details = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin tuần theo ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(WeekDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<WeekDto>> GetById(int id)
        {
            try
            {
                var week = await _weekService.GetByIdAsync(id);
                return Ok(week);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = $"An error occurred while retrieving week with ID {id}.", details = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách tuần theo ID lịch trình.
        /// </summary>
        [HttpGet("BySchedule/{scheduleId}")]
        [ProducesResponseType(typeof(IEnumerable<WeekDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)] // ScheduleId not found
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IEnumerable<WeekDto>>> GetWeeksByScheduleId(int scheduleId)
        {
            try
            {
                var weeks = await _weekService.GetWeeksByScheduleIdAsync(scheduleId);
                // Return 200 OK with an empty list if no weeks are found for a valid scheduleId.
                return Ok(weeks); 
            }
            catch (NotFoundException ex) // Bắt NotFoundException nếu ScheduleId không tồn tại
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = $"An error occurred while retrieving weeks for schedule ID {scheduleId}.", details = ex.Message });
            }
        }

        /// <summary>
        /// Tạo tuần mới.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(WeekDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)] // For invalid foreign keys like ScheduleId
        [ProducesResponseType((int)HttpStatusCode.Conflict)] // For duplicate entries (e.g., WeekNumberInMonth)
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<WeekDto>> CreateWeek([FromBody] CreateWeekDto createWeekDto)
        {
            try
            {
                var createdWeek = await _weekService.AddAsync(createWeekDto);
                return CreatedAtAction(nameof(GetById), new { id = createdWeek.WeekId }, createdWeek);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (NotFoundException ex) // Bắt NotFoundException nếu ScheduleId không tồn tại
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ApiException ex) // Bắt các lỗi nghiệp vụ khác có thể được ném từ service
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred during week creation.", details = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin tuần.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)] // For ID mismatch or validation errors
        [ProducesResponseType((int)HttpStatusCode.Conflict)] // For duplicate entries (e.g., WeekNumberInMonth)
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateWeek(int id, [FromBody] UpdateWeekDto updateWeekDto)
        {
            if (id != updateWeekDto.WeekId)
            {
                return BadRequest(new { message = "ID tuần trong URL không khớp với ID trong body." });
            }

            try
            {
                await _weekService.UpdateAsync(updateWeekDto);
                return NoContent(); // 204 No Content
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
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = $"An error occurred while updating week with ID {id}.", details = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm tuần theo ID lịch trình và/hoặc số tuần trong tháng.
        /// </summary>
        [HttpGet("search")] // Đổi endpoint cho rõ ràng hơn
        [ProducesResponseType(typeof(IEnumerable<WeekDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<IEnumerable<WeekDto>>> SearchWeeks(
            [FromQuery] int? scheduleId, 
            [FromQuery] int? weekNumberInMonth) // Đã đổi tên tham số để khớp với service
        {
            try
            {
                var weeks = await _weekService.SearchWeeksAsync(scheduleId, weekNumberInMonth);
                return Ok(weeks); 
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An error occurred during week search.", details = ex.Message });
            }
        }

        /// <summary>
        /// Xóa tuần theo ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)] // If there are related entities preventing deletion
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _weekService.DeleteAsync(id);
                if (!result) // Mặc dù service nên ném NotFoundException, đây là một fallback an toàn.
                {
                    return NotFound(new { message = $"Week with ID {id} not found." });
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
                    return StatusCode((int)HttpStatusCode.InternalServerError, new { message = $"An error occurred while deleting week with ID {id}.", details = ex.Message });
            }
        }
    }
}