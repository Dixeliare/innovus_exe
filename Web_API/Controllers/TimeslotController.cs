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
    public class TimeslotController : ControllerBase
    {
        private readonly ITimeslotService _timeslotService;
        
        public TimeslotController(ITimeslotService timeslotService) => _timeslotService = timeslotService;

        [HttpGet]
        public async Task<IEnumerable<timeslot>> GetAll()
        {
            return await _timeslotService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<timeslot> GetById(int id)
        {
            return await _timeslotService.GetByIDAsync(id);
        }

        [HttpGet("search_by_start_time_or end_time")]
        public async Task<IEnumerable<timeslot>> SearchByStartTimeOrEndTimeAsync([FromQuery] TimeOnly? startTime,[FromQuery] TimeOnly? endTime)
        {
            return await _timeslotService.SearchByStartTimeOrEndTimeAsync(startTime, endTime);
        }

        [HttpPost]
        public async Task<int> Create(timeslot timeslot)
        {
            return await _timeslotService.CreateTimeslot(timeslot);
        }

        [HttpPut]
        public async Task<int> Update(timeslot timeslot)
        {
            return await _timeslotService.UpdateTimeSlot(timeslot);
        }

        [HttpDelete]
        public async Task<bool> Delete(int id)
        {
            return await _timeslotService.DeleteAsync(id);
        }
    }
}
