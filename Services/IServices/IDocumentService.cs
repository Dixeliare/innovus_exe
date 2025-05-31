using Repository.Models;

namespace Services.IServices;

public interface IDocumentService
{
    Task<IEnumerable<document>> GetAllAsync();
    Task<document> GetByIdAsync(int id);
    Task<int> CreateAsync(document document);
    Task<int> UpdateAsync(document document);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<document>> SearchDocumentsAsync(
        int? lesson = null,
        string? lessonName = null,
        string? link = null,
        int? instrumentId = null);
}