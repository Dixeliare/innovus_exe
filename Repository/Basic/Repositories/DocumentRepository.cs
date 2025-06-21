using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class DocumentRepository : GenericRepository<document>, IDocumentRepository
{
    public DocumentRepository()
    {
    }

    public DocumentRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<document>> GetAllAsync()
    {
        return await _context.documents
            .Include(i => i.instrument)
            .Include(u => u.users)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<document> GetByIdAsync(int id)
    {
        return await _context.documents
            .Include(i => i.instrument)
            .Include(u => u.users)
            .AsSplitQuery()
            .FirstOrDefaultAsync(i => i.document_id == id);
    }

    public async Task<document> AddAsync(document entity)
    {
        _context.documents.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(document entity)
    {
        _context.documents.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.documents.FindAsync(id);
        
        if (item == null) return false;
        
        _context.documents.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<IEnumerable<document>> SearchDocumentsAsync(
        int? lesson = null,
        string? lessonName = null,
        string? link = null,
        int? instrumentId = null)
    {
        IQueryable<document> query = _context.documents;

        // Bao gồm các navigation property nếu muốn eager load thông tin liên quan khi tìm kiếm
        query = query.Include(d => d.instrument);

        // Áp dụng từng điều kiện tìm kiếm nếu tham số được cung cấp
        if (lesson.HasValue)
        {
            query = query.Where(d => d.lesson == lesson.Value);
        }

        if (!string.IsNullOrEmpty(lessonName))
        {
            query = query.Where(d => EF.Functions.ILike(d.lesson_name, $"%{lessonName}%"));
        }

        if (!string.IsNullOrEmpty(link))
        {
            query = query.Where(d => EF.Functions.ILike(d.link, $"%{link}%"));
        }

        if (instrumentId.HasValue)
        {
            query = query.Where(d => d.instrument_id == instrumentId.Value);
        }

        return await query.ToListAsync();
    }
}