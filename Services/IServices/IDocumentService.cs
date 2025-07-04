using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetAllAsync();
    Task<DocumentDto> GetByIdAsync(int id);
    Task<DocumentDto> AddAsync(CreateDocumentDto createDocumentDto);
    Task UpdateAsync(UpdateDocumentDto updateDocumentDto);
    Task DeleteAsync(int id);

    Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(
        int? lesson = null,
        string? lessonName = null,
        string? link = null,
        int? instrumentId = null);
}