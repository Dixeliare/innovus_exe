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
        public async Task<int> Post(attendance attendance)
        {
            return await _attendanceService.CreateAsync(attendance);
        }

        [HttpPut]
        public async Task<int> Put(attendance attendance)
        {
            return await _attendanceService.UpdateAsync(attendance);
        }

        [HttpDelete]
        public async Task<bool> Delete(int id)
        {
            return await _attendanceService.DeleteAsync(id);
        }
    }
}
