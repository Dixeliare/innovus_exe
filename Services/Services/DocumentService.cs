using Repository.Basic.Repositories;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class DocumentService : IDocumentService
{
    private readonly DocumentRepository _documentRepository;
    
    public DocumentService(DocumentRepository documentRepository) => _documentRepository = documentRepository;
    
    public async Task<IEnumerable<document>> GetAllAsync()
    {
        return await _documentRepository.GetAllAsync();
    }

    public async Task<document> GetByIdAsync(int id)
    {
        return await _documentRepository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(document document)
    {
        return await _documentRepository.CreateAsync(document);
    }

    public async Task<int> UpdateAsync(document document)
    {
        return await _documentRepository.UpdateAsync(document);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _documentRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<document>> SearchDocumentsAsync(int? lesson = null, string? lessonName = null, string? link = null, int? instrumentId = null)
    {
        return await _documentRepository.SearchDocumentsAsync(lesson, lessonName, link, instrumentId);
    }
}