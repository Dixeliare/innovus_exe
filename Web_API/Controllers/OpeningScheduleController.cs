using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class OpeningScheduleController : ControllerBase
    {
        private readonly IOpeningScheduleService _openingScheduleService;
        
        public OpeningScheduleController(IOpeningScheduleService openingScheduleService) => _openingScheduleService = openingScheduleService;

        [HttpGet]
        public async Task<IEnumerable<opening_schedule>> GetAllAsync()
        {
            return await _openingScheduleService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<opening_schedule> GetByIdAsync(int id)
        {
            return await _openingScheduleService.GetByIdAsync(id);
        }

        [HttpGet("search_by")]
        public async Task<IEnumerable<opening_schedule>> SearchByAsync([FromQuery] string? subject = null,
            [FromQuery] string? classCode = null, [FromQuery] DateOnly? openingDay = null,
            [FromQuery] DateOnly? endDate = null, [FromQuery] string? schedule = null,
            [FromQuery] int? studentQuantity = null, [FromQuery] bool? isAdvancedClass = null)
        {
            return await _openingScheduleService.SearchOpeningSchedulesAsync(subject, classCode, openingDay, endDate, schedule, studentQuantity, isAdvancedClass);
        }

        [HttpPost]
        public async Task<int> CreateAsync([FromBody] opening_schedule openingSchedule)
        {
            return await _openingScheduleService.CreateAsync(openingSchedule);
        }

        [HttpPut]
        public async Task<int> UpdateAsync([FromBody] opening_schedule openingSchedule)
        {
            return await _openingScheduleService.UpdateAsync(openingSchedule);
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(int id)
        {
            return await _openingScheduleService.DeleteAsync(id);
        }
    }
}
