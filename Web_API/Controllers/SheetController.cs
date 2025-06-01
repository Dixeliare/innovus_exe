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
    public class SheetController : ControllerBase
    {
        private readonly ISheetService _sheetService;
        
        public SheetController(ISheetService sheetService) => _sheetService = sheetService;

        [HttpGet]
        public async Task<IEnumerable<sheet>> GetAllAsync()
        {
            return await _sheetService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SheetDto>> GetSheetById(int id)
        {
            var sheet = await _sheetService.GetByIdAsync(id);
            if (sheet == null)
            {
                return NotFound();
            }
            return Ok(sheet);
        }

        // POST: api/Sheets
        [HttpPost]
        public async Task<ActionResult<SheetDto>> CreateSheet([FromBody] CreateSheetDto createSheetDto)
        {
            try
            {
                var createdSheet = await _sheetService.AddAsync(createSheetDto);
                return CreatedAtAction(nameof(GetSheetById), new { id = createdSheet.SheetId }, createdSheet);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the sheet.", error = ex.Message });
            }
        }

        // PUT: api/Sheets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSheet(int id, [FromBody] UpdateSheetDto updateSheetDto)
        {
            if (id != updateSheetDto.SheetId)
            {
                return BadRequest(new { message = "Sheet ID in URL does not match ID in body." });
            }

            try
            {
                await _sheetService.UpdateAsync(updateSheetDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the sheet.", error = ex.Message });
            }
        }

        // DELETE: api/Sheets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSheet(int id)
        {
            try
            {
                await _sheetService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the sheet.", error = ex.Message });
            }
        }
    }
}
