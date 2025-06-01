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
        public async Task<ActionResult<DocumentDto>> GetDocumentById(int id)
        {
            var document = await _documentService.GetByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return Ok(document);
        }

        [HttpGet("search_by")]
        public async Task<IEnumerable<document>> SearchByDocumentAsync([FromQuery] int? lesson = null,
            [FromQuery] string? lessonName = null, [FromQuery] string? link = null,
            [FromQuery] int? instrumentId = null)
        {
            return await _documentService.SearchDocumentsAsync(lesson, lessonName, link, instrumentId);
        }

        [HttpPost]
        public async Task<ActionResult<DocumentDto>> CreateDocument([FromBody] CreateDocumentDto createDocumentDto)
        {
            try
            {
                var createdDocument = await _documentService.AddAsync(createDocumentDto);
                return CreatedAtAction(nameof(GetDocumentById), new { id = createdDocument.DocumentId }, createdDocument);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the document.", error = ex.Message });
            }
        }

        // PUT: api/Documents/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] UpdateDocumentDto updateDocumentDto)
        {
            if (id != updateDocumentDto.DocumentId)
            {
                return BadRequest(new { message = "Document ID in URL does not match ID in body." });
            }

            try
            {
                await _documentService.UpdateAsync(updateDocumentDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the document.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                await _documentService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the document.", error = ex.Message });
            }
        }
    }
}
