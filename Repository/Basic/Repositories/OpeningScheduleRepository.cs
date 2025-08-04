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
            .Include(d => d.day_of_weeks) // THÊM: Include day_of_weeks collection
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<opening_schedule> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(t => t.teacher_user) 
            .Include(i => i.instrument)
            .Include(d => d.day_of_weeks) // THÊM: Include day_of_weeks collection
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.opening_schedule_id == id);
    }
    
    public async Task<IEnumerable<opening_schedule>> SearchOpeningSchedulesAsync(
        string? classCode = null,
        DateOnly? openingDay = null,
        DateOnly? endDate = null,
        // ĐÃ XÓA: string? schedule = null,
        int? studentQuantity = null,
        bool? isAdvancedClass = null)
    {
        IQueryable<opening_schedule> query = _dbSet
            .Include(t => t.teacher_user)
            .Include(i => i.instrument)
            .Include(d => d.day_of_weeks); // THÊM: Include day_of_weeks collection

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

        // ĐÃ XÓA: if (!string.IsNullOrEmpty(schedule))
        // ĐÃ XÓA: {
        // ĐÃ XÓA:     query = query.Where(o => EF.Functions.ILike(o.schedule, $"%{schedule}%"));
        // ĐÃ XÓA: }

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

    // THÊM: Method để load với day_of_weeks
    public async Task<IEnumerable<opening_schedule>> GetAllWithDayOfWeeksAsync()
    {
        return await _dbSet
            .Include(t => t.teacher_user) 
            .Include(i => i.instrument)
            .Include(d => d.day_of_weeks)
            .AsSplitQuery()
            .ToListAsync();
    }
    
    // THÊM: Method để load với class sessions cho GetAll
    public async Task<IEnumerable<opening_schedule>> GetAllWithClassSessionsAsync()
    {
        return await _dbSet
            .Include(t => t.teacher_user) 
            .Include(i => i.instrument)
            .Include(d => d.day_of_weeks)
            .Include(c => c.class_codeNavigation)
                .ThenInclude(c => c.class_sessions)
                    .ThenInclude(cs => cs.room)
            .Include(c => c.class_codeNavigation)
                .ThenInclude(c => c.class_sessions)
                    .ThenInclude(cs => cs.time_slot)
            .AsSplitQuery()
            .ToListAsync();
    }

    // THÊM: Method để load với day_of_weeks
    public async Task<opening_schedule?> GetByIdWithDayOfWeeksAsync(int id)
    {
        return await _dbSet
            .Include(t => t.teacher_user) 
            .Include(i => i.instrument)
            .Include(d => d.day_of_weeks)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.opening_schedule_id == id);
    }
    
    // THÊM: Method để load với class sessions và related data
    public async Task<opening_schedule?> GetByIdWithClassSessionsAsync(int id)
    {
        return await _dbSet
            .Include(t => t.teacher_user) 
            .Include(i => i.instrument)
            .Include(d => d.day_of_weeks)
            .Include(c => c.class_codeNavigation)
                .ThenInclude(c => c.class_sessions)
                    .ThenInclude(cs => cs.room)
            .Include(c => c.class_codeNavigation)
                .ThenInclude(c => c.class_sessions)
                    .ThenInclude(cs => cs.time_slot)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.opening_schedule_id == id);
    }
}