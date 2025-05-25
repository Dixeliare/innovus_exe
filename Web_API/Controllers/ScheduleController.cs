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
        public async Task<IEnumerable<schedule>> Get()
        {
            return await _scheduleService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<schedule> Get(int id)
        {
            return await _scheduleService.GetByIDAsync(id);
        }
    }
}
