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
using Services.Exceptions;
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
        public async Task<ActionResult<IEnumerable<ConsultationTopicDto>>> SearchConsultationTopicsAsync([FromQuery] string? topicName = null)
        {
            var topics = await _consultationTopicService.SearchConsultationTopicsAsync(topicName);
            return Ok(topics); // Service đã trả về DTOs
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsultationTopicDto>>> GetAllConsultationTopics() // Trả về DTOs
        {
            var topics = await _consultationTopicService.GetAllAsync();
            return Ok(topics);
        }

        // GET: api/ConsultationTopics/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultationTopicDto>> GetConsultationTopicById(int id) // Trả về DTO
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var topic = await _consultationTopicService.GetByIdAsync(id);
            return Ok(topic); // Service đã trả về DTO
        }

        [HttpPost]
        public async Task<ActionResult<ConsultationTopicDto>> CreateConsultationTopic([FromBody] CreateConsultationTopicDto createConsultationTopicDto)
        {
            // Không có try-catch ở đây. Service sẽ ném ValidationException/ApiException nếu có lỗi.
            var createdTopic = await _consultationTopicService.AddAsync(createConsultationTopicDto);
            return CreatedAtAction(nameof(GetConsultationTopicById), new { id = createdTopic.ConsultationTopicId }, createdTopic);
        }

        // PUT: api/ConsultationTopics/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsultationTopic(int id, [FromBody] UpdateConsultationTopicDto updateConsultationTopicDto)
        {
            if (id != updateConsultationTopicDto.ConsultationTopicId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "ConsultationTopicId", new string[] { "ID chủ đề tư vấn trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            await _consultationTopicService.UpdateAsync(updateConsultationTopicDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultationTopic(int id)
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ApiException nếu có lỗi.
            await _consultationTopicService.DeleteAsync(id);
            return NoContent();
        }
    }
}
