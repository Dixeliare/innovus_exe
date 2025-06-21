using Repository.Models;

namespace Repository.Basic.IRepositories;

public interface IDocumentRepository
{
    Task<IEnumerable<document>> GetAllAsync();
    Task<document> GetByIdAsync(int id);
    Task<document> AddAsync(document entity);
    Task UpdateAsync(document entity);
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<document>> SearchDocumentsAsync(
        int? lesson = null,
        string? lessonName = null,
        string? link = null,
        int? instrumentId = null);
}