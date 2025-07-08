using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class ConsultationRequestRepository : GenericRepository<consultation_request>, IConsultationRequestRepository
{
    public ConsultationRequestRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<consultation_request>> GetAllAsync()
    {
        return await _dbSet
            .Include(c => c.consultation_topic)
            .Include(s => s.statistic)
            .Include(h => h.handled_byNavigation)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<consultation_request> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(c => c.consultation_topic)
            .Include(s => s.statistic)
            .Include(h => h.handled_byNavigation)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.consultation_request_id == id);
    }

    // public async Task<consultation_request> AddAsync(consultation_request entity)
    // {
    //     _context.consultation_requests.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(consultation_request entity)
    // {
    //     _context.consultation_requests.Update(entity);
    //     await _context.SaveChangesAsync();
    // }
    //
    // public async Task<bool> DeleteAsync(int id)
    // {
    //     var item = await _context.consultation_requests.FindAsync(id);
    //     
    //     if (item == null) return false;
    //     
    //     _context.consultation_requests.Remove(item);
    //     return await _context.SaveChangesAsync() > 0;
    // }
    
    public async Task<IEnumerable<consultation_request>> SearchConsultationRequestsAsync(
        string? fullname = null,
        string? contactNumber = null,
        string? email = null,
        string? note = null,
        bool? hasContact = null)
    {
        IQueryable<consultation_request> query = _dbSet;

        // Bao gồm các navigation property nếu muốn eager load thông tin liên quan khi tìm kiếm
        query = query.Include(cr => cr.consultation_topic)
            .Include(cr => cr.statistic)
            .Include(cr => cr.handled_byNavigation);

        // Áp dụng từng điều kiện tìm kiếm nếu tham số được cung cấp
        if (!string.IsNullOrEmpty(fullname))
        {
            query = query.Where(cr => EF.Functions.ILike(cr.fullname, $"%{fullname}%"));
        }

        if (!string.IsNullOrEmpty(contactNumber))
        {
            query = query.Where(cr => EF.Functions.ILike(cr.contact_number, $"%{contactNumber}%"));
        }

        if (!string.IsNullOrEmpty(email))
        {
            query = query.Where(cr => EF.Functions.ILike(cr.email, $"%{email}%"));
        }

        if (!string.IsNullOrEmpty(note))
        {
            query = query.Where(cr => EF.Functions.ILike(cr.note, $"%{note}%"));
        }

        if (hasContact.HasValue)
        {
            query = query.Where(cr => cr.has_contact == hasContact.Value);
        }

        return await query.ToListAsync();
    }
}