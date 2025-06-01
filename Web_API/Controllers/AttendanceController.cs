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
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        
        public AttendanceController(IAttendanceService attendanceService) => _attendanceService = attendanceService;

        [HttpGet("search_by_status_or_note")]
        public async Task<IEnumerable<attendance>> SearchAttendancesAsync([FromQuery] bool? status = null,[FromQuery] string? note = null)
        {
            return await _attendanceService.SearchAttendancesAsync(status, note);
        }

        [HttpGet]
        public async Task<IEnumerable<attendance>> GetAll()
        {
            return await _attendanceService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<attendance> GetById(int id)
        {
            return await _attendanceService.GetByIdAsync(id);
        }

        [HttpPost]
        public async Task<ActionResult<AttendanceDto>> Add(CreateAttendanceDto createAttendanceDto)
        {
            try
            {
                var newAttendance = await _attendanceService.AddAsync(createAttendanceDto);
                return CreatedAtAction(nameof(GetById), new { id = newAttendance.AttendanceId }, newAttendance);
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác, ví dụ: khóa ngoại không tồn tại
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAttendanceDto updateAttendanceDto)
        {
            if (id != updateAttendanceDto.AttendanceId)
            {
                return BadRequest("Attendance ID in URL does not match ID in request body.");
            }

            try
            {
                await _attendanceService.UpdateAsync(updateAttendanceDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("id")]
        public async Task<bool> Delete(int id)
        {
            return await _attendanceService.DeleteAsync(id);
        }
    }
}
