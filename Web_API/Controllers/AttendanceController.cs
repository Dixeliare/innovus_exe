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
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        
        public AttendanceController(IAttendanceService attendanceService) => _attendanceService = attendanceService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAllAsync() // Trả về DTOs
        {
            var attendances = await _attendanceService.GetAllAsync();
            return Ok(attendances);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AttendanceDto>> GetById(int id) // Trả về DTO
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var attendance = await _attendanceService.GetByIdAsync(id);
            return Ok(attendance); // Service đã trả về DTO
        }

        [HttpGet("search_by")] // Đổi tên đường dẫn cho rõ ràng hơn
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> SearchAttendancesAsync( // Trả về DTOs
            [FromQuery] bool? status = null,
            [FromQuery] string? note = null)
        {
            var attendances = await _attendanceService.SearchAttendancesAsync(status, note);
            return Ok(attendances); // Service đã trả về DTOs
        }

        [HttpPost]
        public async Task<ActionResult<AttendanceDto>> Add([FromBody] CreateAttendanceDto createAttendanceDto)
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            var newAttendance = await _attendanceService.AddAsync(createAttendanceDto);
            return CreatedAtAction(nameof(GetById), new { id = newAttendance.AttendanceId }, newAttendance);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAttendanceDto updateAttendanceDto)
        {
            if (id != updateAttendanceDto.AttendanceId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "AttendanceId", new[] { "ID điểm danh trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            await _attendanceService.UpdateAsync(updateAttendanceDto);
            return NoContent();
        }

        [HttpDelete("{id}")] // Xóa theo ID từ URL
        public async Task<IActionResult> Delete(int id) // Trả về IActionResult
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ApiException nếu có lỗi.
            await _attendanceService.DeleteAsync(id);
            return NoContent();
        }
    }
}
