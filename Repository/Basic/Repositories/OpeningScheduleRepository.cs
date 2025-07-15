using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class OpeningScheduleRepository : GenericRepository<opening_schedule>, IOpeningScheduleRepository
{
    public OpeningScheduleRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<opening_schedule>> GetAllAsync()
    {
        return await _dbSet
            .Include(t => t.teacher_user) 
            .Include(i => i.instrument)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<opening_schedule> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(t => t.teacher_user) 
            .Include(i => i.instrument)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.opening_schedule_id == id);
    }

    // public async Task<opening_schedule> AddAsync(opening_schedule entity)
    // {
    //     _context.opening_schedules.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(opening_schedule entity)
    // {
    //     _context.opening_schedules.Update(entity);
    //     await _context.SaveChangesAsync();
    // }
    //
    // public async Task<bool> DeleteAsync(int id)
    // {
    //     var item = await _context.opening_schedules.FindAsync(id);
    //     
    //     if (item == null) return false;
    //     _context.opening_schedules.Remove(item);
    //     return await _context.SaveChangesAsync() > 0;
    // }
    
    public async Task<IEnumerable<opening_schedule>> SearchOpeningSchedulesAsync(
        string? classCode = null,
        DateOnly? openingDay = null,
        DateOnly? endDate = null,
        string? schedule = null,
        int? studentQuantity = null,
        bool? isAdvancedClass = null)
    {
        IQueryable<opening_schedule> query = _dbSet
            .Include(t => t.teacher_user)
            .Include(i => i.instrument);

        // Áp dụng từng điều kiện tìm kiếm nếu tham số được cung cấp
        // if (!string.IsNullOrEmpty(subject)) // Đã xóa điều kiện này
        // {
        //     query = query.Where(o => EF.Functions.ILike(o.subject, $"%{subject}%"));
        // }

        if (!string.IsNullOrEmpty(classCode))
        {
            query = query.Where(o => EF.Functions.ILike(o.class_code, $"%{classCode}%"));
        }

        if (openingDay.HasValue)
        {
            query = query.Where(o => o.opening_day == openingDay.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.end_date == endDate.Value);
        }

        if (!string.IsNullOrEmpty(schedule))
        {
            query = query.Where(o => EF.Functions.ILike(o.schedule, $"%{schedule}%"));
        }

        if (studentQuantity.HasValue)
        {
            query = query.Where(o => o.student_quantity == studentQuantity.Value);
        }

        if (isAdvancedClass.HasValue)
        {
            query = query.Where(o => o.is_advanced_class == isAdvancedClass.Value);
        }

        return await query.ToListAsync();
    }
}