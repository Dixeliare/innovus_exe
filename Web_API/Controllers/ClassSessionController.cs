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
    public class ClassSessionController : ControllerBase
    {
        private readonly IClassSessionService _classSessionService;
        
        public ClassSessionController(IClassSessionService classSessionService) => _classSessionService = classSessionService;

        [HttpGet]
        public async Task<IEnumerable<class_session>> GetAllAsync()
        {
            return await _classSessionService.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<class_session> GetAsync(int id)
        {
            return await _classSessionService.GetById(id);
        }

        [HttpGet("search_by_date_or_room_code_or_week_id_or_class_id_or_time_slot_id")]
        public async Task<IEnumerable<class_session>> SearchClassSessions([FromQuery] DateOnly? date = null,[FromQuery] string? roomCode = null,[FromQuery] int? weekId = null,[FromQuery] int? classId = null,
            [FromQuery] int? timeSlotId = null)
        {
            return await _classSessionService.SearchClassSessionsAsync(date, roomCode, weekId, classId, timeSlotId);
        }

        [HttpPost]
        public async Task<int> PostAsync([FromBody] class_session value)
        {
            return await _classSessionService.CreateAsync(value);
        }

        [HttpPut]
        public async Task<int> PutAsync([FromBody] class_session value)
        {
            return await _classSessionService.UpdateAsync(value);
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync([FromBody] int value)
        {
            return await _classSessionService.DeleteAsync(value);
        }
    }
}
