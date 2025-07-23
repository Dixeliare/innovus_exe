using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class ClassSessionRepository : GenericRepository<class_session>, IClassSessionRepository
{
    public ClassSessionRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<class_session>> GetAll()
    {
        var items = await _dbSet
            .Include(c => c._class)
            .Include(a => a.attendances)
            .Include(t => t.time_slot)
            .Include(d=>d.day)
            .AsSplitQuery()
            .ToListAsync();
        
        return items ?? new List<class_session>();
    }

    public async Task<class_session> GetByIdAsync(int id)
    {
        var item = await _dbSet
            .Include(c => c._class)
            .Include(a => a.attendances)
            .Include(t => t.time_slot)
            .Include(d=>d.day)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.class_session_id == id);
        return item ?? new class_session();
    }

    public async Task<IEnumerable<class_session>> GetAllClassSessionsWithDetailsAsync()
    {
        return await _dbSet
            .Include(cs => cs._class) // Using _class
                .ThenInclude(c => c.instrument) // Assuming _class has an Instrument navigation property
            .Include(cs => cs.day)
                .ThenInclude(d => d.week) // Include Day's Week
            .Include(cs => cs.time_slot)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<class_session?> GetClassSessionByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(cs => cs._class)
                .ThenInclude(c => c.instrument)
            .Include(cs => cs.day)
                .ThenInclude(d => d.week)
            .Include(cs => cs.time_slot)
            .FirstOrDefaultAsync(cs => cs.class_session_id == id);
    }

    public async Task<IEnumerable<class_session>> GetClassSessionsByClassIdWithDetailsAsync(int classId)
    {
        return await _dbSet
            .Where(cs => cs.class_id == classId)
            .Include(cs => cs._class)
                .ThenInclude(c => c.instrument)
            .Include(cs => cs.day)
                .ThenInclude(d => d.week)
            .Include(cs => cs.time_slot)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<class_session>> GetClassSessionsByDayIdWithDetailsAsync(int dayId)
    {
        return await _dbSet
            .Where(cs => cs.day_id == dayId)
            .Include(cs => cs._class)
                .ThenInclude(c => c.instrument)
            .Include(cs => cs.day)
                .ThenInclude(d => d.week)
            .Include(cs => cs.time_slot)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<class_session>> SearchClassSessionsWithDetailsAsync(
        int? sessionNumber = null,
        DateOnly? date = null,
        string? roomCode = null,
        int? classId = null,
        int? dayId = null,
        int? timeSlotId = null)
    {
        IQueryable<class_session> query = _dbSet;

        if (sessionNumber.HasValue)
        {
            query = query.Where(cs => cs.session_number == sessionNumber.Value);
        }
        if (date.HasValue)
        {
            query = query.Where(cs => cs.date == date.Value);
        }
        if (!string.IsNullOrWhiteSpace(roomCode))
        {
            query = query.Where(cs => cs.room_code.Contains(roomCode));
        }
        if (classId.HasValue)
        {
            query = query.Where(cs => cs.class_id == classId.Value);
        }
        if (dayId.HasValue)
        {
            query = query.Where(cs => cs.day_id == dayId.Value);
        }
        if (timeSlotId.HasValue)
        {
            query = query.Where(cs => cs.time_slot_id == timeSlotId.Value);
        }

        return await query
            .Include(cs => cs._class)
                .ThenInclude(c => c.instrument)
            .Include(cs => cs.day)
                .ThenInclude(d => d.week)
            .Include(cs => cs.time_slot)
            .AsSplitQuery()
            .ToListAsync();
    }

    // Search method without eager loading (for internal use, like uniqueness checks)
    public async Task<IEnumerable<class_session>> SearchClassSessionsAsync(
        int? sessionNumber = null,
        DateOnly? date = null,
        string? roomCode = null,
        int? classId = null,
        int? dayId = null,
        int? timeSlotId = null)
    {
        IQueryable<class_session> query = _dbSet;

        if (sessionNumber.HasValue)
        {
            query = query.Where(cs => cs.session_number == sessionNumber.Value);
        }
        if (date.HasValue)
        {
            query = query.Where(cs => cs.date == date.Value);
        }
        if (!string.IsNullOrWhiteSpace(roomCode))
        {
            query = query.Where(cs => cs.room_code.Contains(roomCode));
        }
        if (classId.HasValue)
        {
            query = query.Where(cs => cs.class_id == classId.Value);
        }
        if (dayId.HasValue)
        {
            query = query.Where(cs => cs.day_id == dayId.Value);
        }
        if (timeSlotId.HasValue)
        {
            query = query.Where(cs => cs.time_slot_id == timeSlotId.Value);
        }

        return await query.ToListAsync();
    }
}