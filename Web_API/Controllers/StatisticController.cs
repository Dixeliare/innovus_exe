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
    }
}
