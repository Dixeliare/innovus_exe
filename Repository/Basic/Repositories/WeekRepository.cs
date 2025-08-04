using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class WeekRepository : GenericRepository<week>, IWeekRepository
{

    public WeekRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<week>> GetAllWeeksWithDaysAsync()
    {
        return await _dbSet
            .Include(w => w.days)
            .Include(s => s.schedule)
            .ToListAsync();
    }

    public async Task<week?> GetWeekByIdWithDaysAsync(int id)
    {
        return await _dbSet
            .Include(w => w.days)
            .Include(s => s.schedule)
            .FirstOrDefaultAsync(w => w.week_id == id);
    }

    public async Task<IEnumerable<week>> GetWeeksByScheduleIdWithDaysAsync(int scheduleId)
    {
        return await _dbSet
            .Where(w => w.schedule_id == scheduleId)
            .Include(w => w.days)
            .Include(s => s.schedule)
            .ToListAsync();
    }

    public async Task<week?> GetWeekByIdWithDaysAndClassSessionsAsync(int id)
    {
        return await _dbSet
            .Include(w => w.days)
                .ThenInclude(d => d.class_sessions) 
            .Include(s => s.schedule)// Include class sessions for each day
            .FirstOrDefaultAsync(w => w.week_id == id);
    }

    public async Task<IEnumerable<week>> GetWeeksByScheduleIdWithDaysAndClassSessionsAsync(int scheduleId)
    {
        return await _dbSet
            .Where(w => w.schedule_id == scheduleId)
            .Include(w => w.days)
                .ThenInclude(d => d.class_sessions)
            .Include(s => s.schedule)// Include class sessions for each day
            .ToListAsync();
    }

    public async Task<IEnumerable<week>> SearchWeeksAsync(int? scheduleId = null, int? weekNumberInMonth = null, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        IQueryable<week> query = _dbSet;

        if (scheduleId.HasValue)
        {
            query = query.Where(w => w.schedule_id == scheduleId.Value);
        }

        if (weekNumberInMonth.HasValue)
        {
            query = query.Where(w => w.week_number_in_month == weekNumberInMonth.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(w => w.start_date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(w => w.end_date <= endDate.Value);
        }

        // Include Days by default for search, or provide another method without Days
        query = query.Include(w => w.days);

        return await query.ToListAsync();
    }
    
    // Keep this method as it might be used elsewhere for simple week retrieval without days
    public async Task<IEnumerable<week>> GetWeeksByScheduleIdAsync(int scheduleId)
    {
        return await _dbSet.Where(w => w.schedule_id == scheduleId).ToListAsync();
    }

    public async Task<IEnumerable<week>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(w => w.schedule)
            .Include(w => w.days)
                .ThenInclude(d => d.class_sessions)
                    .ThenInclude(cs => cs.room)
            .Include(w => w.days)
                .ThenInclude(d => d.class_sessions)
                    .ThenInclude(cs => cs._class)
            .Include(w => w.days)
                .ThenInclude(d => d.class_sessions)
                    .ThenInclude(cs => cs.time_slot)
            .ToListAsync();
    }
}