using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class ConsultationTopicRepository : GenericRepository<consultation_topic>, IConsultationTopicRepository
{
    public ConsultationTopicRepository()
    {
    }
    
    public ConsultationTopicRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<consultation_topic>> GetAllAsync()
    {
        return await _context.consultation_topics
            .Include(c => c.consultation_requests)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<consultation_topic> GetByIdAsync(int id)
    {
        return await _context.consultation_topics
            .Include(c => c.consultation_requests)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.consultation_topic_id == id);
    }

    public async Task<consultation_topic> AddAsync(consultation_topic entity)
    {
        _context.consultation_topics.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(consultation_topic entity)
    {
        _context.consultation_topics.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.consultation_topics.FindAsync(id);

        if (item == null)
        {
            return false;
        }
        _context.consultation_topics.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<consultation_topic>> SearchConsultationTopicsAsync(string? topicName = null)
    {
        IQueryable<consultation_topic> query = _context.consultation_topics;

        // Áp dụng điều kiện tìm kiếm nếu topicName được cung cấp
        if (!string.IsNullOrEmpty(topicName))
        {
            // Sử dụng EF.Functions.ILike cho tìm kiếm không phân biệt chữ hoa/thường và khớp một phần
            query = query.Where(t => EF.Functions.ILike(t.consultation_topic_name, $"%{topicName}%"));
        }
        // Nếu topicName là null hoặc rỗng, query sẽ không bị lọc và trả về tất cả.

        // Bạn có thể thêm .Include() nếu muốn eager load các navigation properties
        // Ví dụ: .Include(t => t.consultation_requests)

        return await query.ToListAsync();
    }
}