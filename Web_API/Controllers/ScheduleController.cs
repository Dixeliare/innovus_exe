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
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService? _scheduleService;
        
        public ScheduleController(IScheduleService? scheduleService) => _scheduleService = scheduleService;

        [HttpGet]
        public async Task<IEnumerable<schedule>> GetAll()
        {
            return await _scheduleService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<schedule> GetById(int id)
        {
            return await _scheduleService.GetByIDAsync(id);
        }

        [HttpGet("({id} / {note})")]
        public async Task<IEnumerable<schedule>> SearchByIdOrNote(int id, string note)
        {
            return await _scheduleService.SearchByIdOrNoteAsync(id, note);
        }

        [HttpPost]
        public async Task<int> Create(schedule schedule)
        {
            return await _scheduleService.CreateSchedule(schedule);
        }

        [HttpPut]
        public async Task<int> Update(schedule schedule)
        {
            return await _scheduleService.UpdateSchedule(schedule);
        }

        [HttpDelete("{id}")]
        public async Task<bool> Delete(int id)
        {
            return await _scheduleService.DeleteAsync(id);
        }
    }
}
