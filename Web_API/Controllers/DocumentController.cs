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
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        
        public DocumentController(IDocumentService documentService) => _documentService = documentService;

        [HttpGet]
        public async Task<IEnumerable<document>> GetAllAsync()
        {
            return await _documentService.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<document> GetByIdAsync(int id)
        {
            return await _documentService.GetByIdAsync(id);
        }

        [HttpGet("search_by")]
        public async Task<IEnumerable<document>> SearchByDocumentAsync([FromQuery] int? lesson = null,
            [FromQuery] string? lessonName = null, [FromQuery] string? link = null,
            [FromQuery] int? instrumentId = null)
        {
            return await _documentService.SearchDocumentsAsync(lesson, lessonName, link, instrumentId);
        }

        [HttpPost]
        public async Task<int> CreateAsync(document document)
        {
            return await _documentService.CreateAsync(document);
        }

        [HttpPut]
        public async Task<int> UpdateAsync(document document)
        {
            return await _documentService.UpdateAsync(document);
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(int id)
        {
            return await _documentService.DeleteAsync(id);
        }
    }
}
