using DTOs;
using Repository.Models;

namespace Services.IServices;

public interface IDocumentService
{
    Task<IEnumerable<document>> GetAllAsync();
    Task<document> GetByIdAsync(int id);
    Task<DocumentDto> AddAsync(CreateDocumentDto createDocumentDto);
    Task UpdateAsync(UpdateDocumentDto updateDocumentDto);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<document>> SearchDocumentsAsync(
        int? lesson = null,
        string? lessonName = null,
        string? link = null,
        int? instrumentId = null);
}