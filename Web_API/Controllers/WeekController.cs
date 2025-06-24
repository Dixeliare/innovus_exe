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
            return await _weekService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<week> GetById(int id)
        {
            return await _weekService.GetByIdAsync(id);
        }

        [HttpGet("BySchedule/{scheduleId}")]
        public async Task<ActionResult<IEnumerable<WeekDto>>> GetWeeksByScheduleId(int scheduleId)
        {
            // Nếu scheduleId không tồn tại, WeekService sẽ ném NotFoundException,
            // Middleware sẽ bắt và trả về 404.
            var weeks = await _weekService.GetWeeksByScheduleIdAsync(scheduleId);
            
            // Bạn có thể chọn cách xử lý nếu danh sách rỗng:
            // 1. Trả về 200 OK với mảng rỗng (thường là phổ biến nhất nếu đây là kết quả hợp lệ)
            if (!weeks.Any())
            {
                return Ok(new List<WeekDto>()); // Trả về 200 OK với danh sách rỗng
            }
            return Ok(weeks);

            // 2. Hoặc trả về 404 Not Found nếu bạn coi việc không có tuần nào là lỗi
            // if (!weeks.Any())
            // {
            //     throw new NotFoundException($"No weeks found for schedule ID {scheduleId}.");
            // }
            // return Ok(weeks);
        }

        // POST: api/Weeks
        [HttpPost]
        public async Task<ActionResult<WeekDto>> CreateWeek([FromBody] CreateWeekDto createWeekDto)
        {
            // XÓA KHỐI TRY-CATCH NÀY!
            // Middleware sẽ bắt KeyNotFoundException (thay thế bằng NotFoundException),
            // ValidationException, hoặc ApiException và trả về phản hồi thích hợp.
            // try
            // {
            var createdWeek = await _weekService.AddAsync(createWeekDto);
            // CreatedAtAction sẽ trả về HTTP 201 Created
            return CreatedAtAction(nameof(GetById), new { id = createdWeek.WeekId }, createdWeek);
            // }
            // catch (KeyNotFoundException ex) { return BadRequest(new { message = ex.Message }); }
            // catch (Exception ex) { return StatusCode(500, new { message = "...", error = ex.Message }); }
        }

        // PUT: api/Weeks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWeek(int id, [FromBody] UpdateWeekDto updateWeekDto)
        {
            if (id != updateWeekDto.WeekId)
            {
                // Thay vì BadRequest, bạn có thể ném ValidationException ở đây hoặc trong Service
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "WeekId", new string[] { "ID tuần trong URL không khớp với ID trong body." } }
                });
                // return BadRequest(new { message = "Week ID in URL does not match ID in body." });
            }

            // XÓA KHỐI TRY-CATCH NÀY!
            // Middleware sẽ bắt NotFoundException, ValidationException, ApiException và trả về phản hồi thích hợp.
            // try
            // {
            await _weekService.UpdateAsync(updateWeekDto);
            // NoContent() trả về HTTP 204 No Content (thành công nhưng không có nội dung trả về)
            return NoContent();
            // }
            // catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            // catch (Exception ex) { return StatusCode(500, new { message = "...", error = ex.Message }); }
        }

        [HttpGet("search_by_day_of_week_or_schedule_id")]
        public async Task<IEnumerable<week>> SearchByDayOfWeekOrScheduleId([FromQuery] DateOnly? dayOfWeek,[FromQuery] int? scheduleId)
        {
            return await _weekService.SearchWeeksAsync(dayOfWeek, scheduleId);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            // XÓA KHỐI TRY-CATCH NÀY!
            // Middleware sẽ bắt NotFoundException, ApiException và trả về phản hồi thích hợp.
            // try
            // {
            var deleted = await _weekService.DeleteAsync(id);
            if (deleted)
            {
                return NoContent(); // HTTP 204 No Content
            }
            else
            {
                // Nếu DeleteAsync trả về false (do không tìm thấy), Service đã ném NotFoundException
                // Vậy nên đoạn code này có thể không bao giờ được chạy nếu Service ném Exception
                // Hoặc bạn có thể ném một lỗi ở đây nếu service không ném mà chỉ trả về false
                // throw new NotFoundException("Week", "Id", id); // Ví dụ
                return NotFound(new { message = $"Week with ID {id} not found or could not be deleted." });
            }
            // }
            // catch (Exception ex)
            // {
            //     return StatusCode(500, new { message = "An error occurred while deleting the week.", error = ex.Message });
            // }
        }
    }
}