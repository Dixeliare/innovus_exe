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
    public class ConsultationRequestController : ControllerBase
    {
        private readonly IConsultationRequestService _consultationRequestService;
        
        public ConsultationRequestController(IConsultationRequestService consultationRequestService) => _consultationRequestService = consultationRequestService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsultationRequestDto>>> GetAllConsultationRequests()
        {
            var requests = await _consultationRequestService.GetAllAsync();
            return Ok(requests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultationRequestDto>> GetConsultationRequestById(int id)
        {
            var request = await _consultationRequestService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            return Ok(request);
        }

        [HttpPost]
        public async Task<ActionResult<ConsultationRequestDto>> CreateConsultationRequest([FromBody] CreateConsultationRequestDto createConsultationRequestDto)
        {
            try
            {
                var createdRequest = await _consultationRequestService.AddAsync(createConsultationRequestDto);
                return CreatedAtAction(nameof(GetConsultationRequestById), new { id = createdRequest.ConsultationRequestId }, createdRequest);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the consultation request.", error = ex.Message });
            }
        }

        // PUT: api/ConsultationRequests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsultationRequest(int id, [FromBody] UpdateConsultationRequestDto updateConsultationRequestDto)
        {
            if (id != updateConsultationRequestDto.ConsultationRequestId)
            {
                return BadRequest(new { message = "Consultation Request ID in URL does not match ID in body." });
            }

            try
            {
                await _consultationRequestService.UpdateAsync(updateConsultationRequestDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the consultation request.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultationRequest(int id)
        {
            try
            {
                await _consultationRequestService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the consultation request.", error = ex.Message });
            }
        }

        [HttpGet("search_by")]
        public async Task<IEnumerable<consultation_request>> SearchByAsync([FromQuery] string? fullname = null,
            [FromQuery] string? contactNumber = null,
            [FromQuery] string? email = null,
            [FromQuery] string? note = null,
            [FromQuery] bool? hasContact = null)
        {
            return await _consultationRequestService.SearchConsultationRequestsAsync(fullname, contactNumber, email, note, hasContact);
        }
    }
}
