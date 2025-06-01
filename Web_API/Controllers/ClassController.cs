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
using Services.IServices;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        
        public ClassController(IClassService classService) => _classService = classService;

        [HttpGet]
        public async Task<IEnumerable<_class>> GetAll()
        {
            return await _classService.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<_class> GetById(int id)
        {
            return await _classService.GetById(id);
        }

        [HttpPost]
        public async Task<ActionResult<ClassDto>> Add(CreateClassDto createClassDto)
        {
            var newClass = await _classService.AddAsync(createClassDto);
            // Trả về 201 CreatedAtAction nếu thành công
            return CreatedAtAction(nameof(GetById), new { id = newClass.ClassId }, newClass);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateClassDto updateClassDto)
        {
            if (id != updateClassDto.ClassId)
            {
                return BadRequest("Class ID in URL does not match ID in request body.");
            }

            try
            {
                await _classService.UpdateAsync(updateClassDto);
                return NoContent(); // 204 No Content cho cập nhật thành công
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); // Trả về 404 nếu không tìm thấy lớp học
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("id")]
        public async Task<bool> DeleteAsync(int id)
        {
            return await _classService.DeleteAsync(id);
        }

        [HttpGet("search_by_instrumet_id_or_class_code")]
        public async Task<IEnumerable<_class>> SearchAsync([FromQuery] int? instrumentId = null,[FromQuery] string? classCode = null)
        {
            return await _classService.SearchClassesAsync(instrumentId, classCode);
        }
    }
}
