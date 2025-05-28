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
        public async Task<IEnumerable<consultation_topic>> GetAll()
        {
            return await _consultationTopicService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<consultation_topic> GetById(int id)
        {
            return await _consultationTopicService.GetByIdAsync(id);
        }

        [HttpPost]
        public async Task<int> Create(consultation_topic consultation_topic)
        {
            return await _consultationTopicService.CreateAsync(consultation_topic);
        }

        [HttpPut]
        public async Task<int> Update(consultation_topic consultation_topic)
        {
            return await _consultationTopicService.UpdateAsync(consultation_topic);
        }

        [HttpDelete("{id}")]
        public async Task<bool> Delete(int id)
        {
            return await _consultationTopicService.DeleteAsync(id);
        }
    }
}
