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
    public class ConsultationRequestController : ControllerBase
    {
        private readonly IConsultationRequestService _consultationRequestService;
        
        public ConsultationRequestController(IConsultationRequestService consultationRequestService) => _consultationRequestService = consultationRequestService;

        [HttpGet]
        public async Task<IEnumerable<consultation_request>> GetAllAsync()
        {
            return await _consultationRequestService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<consultation_request> GetByIdAsync(int id)
        {
            return await _consultationRequestService.GetByIdAsync(id);
        }

        [HttpPost]
        public async Task<int> PostAsync([FromBody] consultation_request consultation_request)
        {
            return await _consultationRequestService.CreateAsync(consultation_request);
        }

        [HttpPut]
        public async Task<int> PutAsync([FromBody] consultation_request consultation_request)
        {
            return await _consultationRequestService.UpdateAsync(consultation_request);
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(int id)
        {
            return await _consultationRequestService.DeleteAsync(id);
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
