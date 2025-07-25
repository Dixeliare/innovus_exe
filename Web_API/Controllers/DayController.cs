using DTOs;
using Microsoft.AspNetCore.Mvc;
using Services.IServices;

namespace Web_API.Controllers;

[ApiController]
    [Route("api/[controller]")]
    public class DayController : ControllerBase
    {
        private readonly IDayService _dayService;

        public DayController(IDayService dayService)
        {
            _dayService = dayService;
        }

        /// <summary>
        /// Lấy tất cả các ngày.
        /// </summary>
        /// <returns>Danh sách các ngày.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DayDto>), 200)]
        public async Task<IActionResult> GetAllDays()
        {
            var days = await _dayService.GetAllDaysAsync();
            return Ok(days);
        }

        /// <summary>
        /// Lấy thông tin ngày theo ID.
        /// </summary>
        /// <param name="id">ID của ngày.</param>
        /// <returns>Thông tin ngày hoặc NotFound nếu không tìm thấy.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DayDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetDayById(int id)
        {
            var day = await _dayService.GetDayByIdAsync(id);
            if (day == null)
            {
                return NotFound($"Day with ID {id} not found.");
            }
            return Ok(day);
        }

        /// <summary>
        /// Tìm kiếm các ngày theo tiêu chí.
        /// </summary>
        /// <param name="dateOfDay">Ngày cụ thể.</param>
        /// <param name="weekId">ID tuần.</param>
        /// <param name="dayOfWeekName">Tên ngày trong tuần (e.g., "Monday").</param>
        /// <returns>Danh sách các ngày phù hợp.</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<DayDto>), 200)]
        public async Task<IActionResult> SearchDays([FromQuery] DateOnly? dateOfDay, [FromQuery] int? weekId, [FromQuery] string? dayOfWeekName)
        {
            var days = await _dayService.SearchDaysAsync(dateOfDay, weekId, dayOfWeekName);
            return Ok(days);
        }

        /// <summary>
        /// Tạo một ngày mới.
        /// </summary>
        /// <param name="createDayDto">Thông tin ngày cần tạo.</param>
        /// <returns>Ngày đã được tạo.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(DayDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateDay([FromBody] CreateDayDto createDayDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newDay = await _dayService.CreateDayAsync(createDayDto);
                return CreatedAtAction(nameof(GetDayById), new { id = newDay.DayId }, newDay);
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết (sử dụng ILogger)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin một ngày hiện có.
        /// </summary>
        /// <param name="updateDayDto">Thông tin ngày cần cập nhật.</param>
        /// <returns>NoContent nếu thành công, NotFound nếu không tìm thấy, BadRequest nếu lỗi.</returns>
        [HttpPut]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateDay([FromBody] UpdateDayDto updateDayDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _dayService.UpdateDayAsync(updateDayDto);
            if (!result)
            {
                return NotFound($"Day with ID {updateDayDto.DayId} not found or could not be updated.");
            }
            return NoContent();
        }

        /// <summary>
        /// Xóa một ngày theo ID.
        /// </summary>
        /// <param name="id">ID của ngày cần xóa.</param>
        /// <returns>NoContent nếu thành công, NotFound nếu không tìm thấy.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteDay(int id)
        {
            var result = await _dayService.DeleteDayAsync(id);
            if (!result)
            {
                return NotFound($"Day with ID {id} not found.");
            }
            return NoContent();
        }
    }