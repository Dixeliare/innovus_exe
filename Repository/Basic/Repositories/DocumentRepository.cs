using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class DocumentRepository : GenericRepository<document>
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
            .ToListAsync();
    }

    public async Task<document> GetByIdAsync(int id)
    {
        return await _context.documents
            .Include(i => i.instrument)
            .Include(u => u.users)
            .FirstOrDefaultAsync(i => i.document_id == id);
    }

    public async Task<int> CreateAsync(document document)
    {
        await _context.documents.AddAsync(document);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(document document)
    {
        var item = await _context.documents.FindAsync(document.document_id);
        
        if (item == null) return 0;
        
        item.lesson = document.lesson;
        item.lesson_name = document.lesson_name;
        item.link = document.link;
        item.instrument_id = document.instrument_id;
        
        return await _context.SaveChangesAsync();
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