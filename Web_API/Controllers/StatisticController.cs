using System;
using System.Collections.Generic;
using System.Linq;
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
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticService _statisticService;

        public StatisticController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        // GET: api/Statistic
        [HttpGet]
        public async Task<ActionResult<IEnumerable<statistic>>> GetStatistic()
        {
            var statistics = await _statisticService.GetAllAsync();
            return Ok(statistics);
        }

        // GET: api/Statistic/5
        [HttpGet("{id}")]
        public async Task<ActionResult<statistic>> Getstatistic(int id)
        {
            var statistic = await _statisticService.GetByIdAsync(id);
            return Ok(statistic);
        }

        // GET: api/Statistic/month/2025/8
        [HttpGet("month/{year}/{month}")]
        public async Task<ActionResult<StatisticDto>> GetStatisticByMonth(int year, int month)
        {
            try
            {
                var statistic = await _statisticService.GetByMonthAsync(year, month);
                return Ok(statistic);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/Statistic/current-month
        [HttpGet("current-month")]
        public async Task<ActionResult<StatisticDto>> GetCurrentMonthStatistic()
        {
            try
            {
                var statistic = await _statisticService.GetCurrentMonthAsync();
                return Ok(statistic);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thống kê tháng hiện tại", error = ex.Message });
            }
        }

        // PUT: api/Statistic/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putstatistic(int id, [FromBody] UpdateStatisticDto updateStatisticDto)
        {
            if (id != updateStatisticDto.StatisticId)
            {
                // Ném ValidationException thay vì BadRequest
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "StatisticId", new string[] { "ID thống kê trong URL không khớp với ID trong body." } }
                });
            }

            // XÓA KHỐI TRY-CATCH VÀ LOGIC CHECK statisticsExists NÀY!
            await _statisticService.UpdateAsync(updateStatisticDto);
            return NoContent();
        }

        // POST: api/Statistic
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<statistic>> Poststatistic([FromBody] CreateStatisticDto createStatisticDto)
        {
            var createdStatistic = await _statisticService.AddAsync(createStatisticDto);
            return CreatedAtAction(nameof(GetStatistic), new { id = createdStatistic.StatisticId }, createdStatistic);

        }

        // DELETE: api/Statistic/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletestatistic(int id)
        {
            await _statisticService.DeleteAsync(id);
            return NoContent();
        }

        // POST: api/Statistic/update-statistics
        // Endpoint để trigger cập nhật thống kê tự động
        [HttpPost("update-statistics")]
        public async Task<IActionResult> UpdateStatistics()
        {
            try
            {
                await _statisticService.UpdateStatisticsAsync();
                return Ok(new { message = "Thống kê đã được cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi cập nhật thống kê", error = ex.Message });
            }
        }

        // POST: api/Statistic/update-user-statistics
        // Endpoint để cập nhật thống kê liên quan đến user
        [HttpPost("update-user-statistics")]
        public async Task<IActionResult> UpdateUserStatistics()
        {
            try
            {
                await _statisticService.UpdateStatisticsOnUserChangeAsync();
                return Ok(new { message = "Thống kê user đã được cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi cập nhật thống kê user", error = ex.Message });
            }
        }

        // POST: api/Statistic/update-class-statistics
        // Endpoint để cập nhật thống kê liên quan đến class
        [HttpPost("update-class-statistics")]
        public async Task<IActionResult> UpdateClassStatistics()
        {
            try
            {
                await _statisticService.UpdateStatisticsOnClassChangeAsync();
                return Ok(new { message = "Thống kê class đã được cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi cập nhật thống kê class", error = ex.Message });
            }
        }

        // GET: api/Statistic/realtime
        // Endpoint để lấy thống kê realtime của tháng hiện tại
        [HttpGet("realtime")]
        public async Task<ActionResult<StatisticDto>> GetRealtimeStatistics()
        {
            try
            {
                // Cập nhật thống kê realtime trước khi trả về
                await _statisticService.UpdateStatisticsAsync();
                
                // Lấy thống kê tháng hiện tại
                var currentMonthStatistic = await _statisticService.GetCurrentMonthAsync();
                
                if (currentMonthStatistic == null)
                {
                    return NotFound(new { message = "Không tìm thấy thống kê cho tháng hiện tại" });
                }
                
                return Ok(currentMonthStatistic);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Có lỗi xảy ra khi lấy thống kê realtime", error = ex.Message });
            }
        }
    }
}
