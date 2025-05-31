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
    public class WeekController : ControllerBase
    {
        private readonly IWeekService _weekService;
        
        public WeekController(IWeekService weekService) => _weekService = weekService;
        
        [HttpGet]
        public async Task<IEnumerable<week>> GetAll()
        {
            return await _weekService.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<week> GetById(int id)
        {
            return await _weekService.GetById(id);
        }

        [HttpPost]
        public async Task<int> Post([FromBody] week value)
        {
            return await _weekService.CreateAsync(value);
        }

        [HttpPut]
        public async Task<int> Put([FromBody] week value)
        {
            return await _weekService.UpdateAsync(value);
        }

        [HttpGet("search_by_day_of_week_or_schedule_id")]
        public async Task<IEnumerable<week>> SearchByDayOfWeekOrScheduleId([FromQuery] DateOnly? dayOfWeek,[FromQuery] int? scheduleId)
        {
            return await _weekService.SearchWeeksAsync(dayOfWeek, scheduleId);
        }

        [HttpDelete("{id}")]
        public async Task<bool> Delete([FromBody] int id)
        {
            return await _weekService.DeleteAsync(id);
        }
    }
}