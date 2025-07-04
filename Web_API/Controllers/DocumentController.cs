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
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        
        public DocumentController(IDocumentService documentService) => _documentService = documentService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAllAsync() // Trả về DTO
        {
            var documents = await _documentService.GetAllAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocumentById(int id) // Trả về DTO
        {
            // Service sẽ ném NotFoundException nếu không tìm thấy
            var document = await _documentService.GetByIdAsync(id);
            return Ok(document); // Service đã trả về DTO
        }

        [HttpGet("search_by")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> SearchByDocumentAsync([FromQuery] int? lesson = null,
            [FromQuery] string? lessonName = null, [FromQuery] string? link = null,
            [FromQuery] int? instrumentId = null)
        {
            var documents = await _documentService.SearchDocumentsAsync(lesson, lessonName, link, instrumentId);
            return Ok(documents); // Service đã trả về DTOs
        }

        [HttpPost]
        public async Task<ActionResult<DocumentDto>> CreateDocument([FromBody] CreateDocumentDto createDocumentDto)
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            var createdDocument = await _documentService.AddAsync(createDocumentDto);
            return CreatedAtAction(nameof(GetDocumentById), new { id = createdDocument.DocumentId }, createdDocument);
        }

        // PUT: api/Documents/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] UpdateDocumentDto updateDocumentDto)
        {
            if (id != updateDocumentDto.DocumentId)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "DocumentId", new string[] { "ID tài liệu trong URL không khớp với ID trong body." } }
                });
            }

            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ValidationException/ApiException nếu có lỗi.
            await _documentService.UpdateAsync(updateDocumentDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            // Không có try-catch ở đây. Service sẽ ném NotFoundException/ApiException nếu có lỗi.
            await _documentService.DeleteAsync(id);
            return NoContent();
        }
    }
}
