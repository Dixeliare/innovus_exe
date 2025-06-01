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
    public class ConsultationTopicController : ControllerBase
    {
        private readonly IConsultationTopicService _consultationTopicService;
        
        public ConsultationTopicController(IConsultationTopicService consultationTopicService) => _consultationTopicService = consultationTopicService;

        [HttpGet("search_by_consultation_topic_name")]
        public async Task<IEnumerable<consultation_topic>> SearchConsultationTopicsAsync(string? topicName = null)
        {
            return await _consultationTopicService.SearchConsultationTopicsAsync(topicName);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsultationTopicDto>>> GetAllConsultationTopics()
        {
            var topics = await _consultationTopicService.GetAllAsync();
            return Ok(topics);
        }

        // GET: api/ConsultationTopics/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultationTopicDto>> GetConsultationTopicById(int id)
        {
            var topic = await _consultationTopicService.GetByIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }
            return Ok(topic);
        }


        [HttpPost]
        public async Task<ActionResult<ConsultationTopicDto>> CreateConsultationTopic([FromBody] CreateConsultationTopicDto createConsultationTopicDto)
        {
            try
            {
                var createdTopic = await _consultationTopicService.AddAsync(createConsultationTopicDto);
                return CreatedAtAction(nameof(GetConsultationTopicById), new { id = createdTopic.ConsultationTopicId }, createdTopic);
            }
            catch (Exception ex)
            {
                // Vì không có khóa ngoại, lỗi thường là do validation hoặc DB
                return StatusCode(500, new { message = "An error occurred while creating the consultation topic.", error = ex.Message });
            }
        }

        // PUT: api/ConsultationTopics/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsultationTopic(int id, [FromBody] UpdateConsultationTopicDto updateConsultationTopicDto)
        {
            if (id != updateConsultationTopicDto.ConsultationTopicId)
            {
                return BadRequest(new { message = "Consultation Topic ID in URL does not match ID in body." });
            }

            try
            {
                await _consultationTopicService.UpdateAsync(updateConsultationTopicDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the consultation topic.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultationTopic(int id)
        {
            try
            {
                await _consultationTopicService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the consultation topic.", error = ex.Message });
            }
        }
    }
}
