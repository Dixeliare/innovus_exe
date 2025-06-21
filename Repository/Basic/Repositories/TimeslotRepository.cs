using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class TimeslotRepository : GenericRepository<timeslot>, ITimeslotRepository
{
    public TimeslotRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<timeslot>> GetAllAsync()
    {
        var items = await _dbSet
            .Include(c => c.class_sessions)
            .AsSplitQuery()
            .ToListAsync();
        return items ?? new List<timeslot>();
    }

    public async Task<timeslot> GetByIdAsync(int id)
    {
        var item = await _dbSet
            .Include(c => c.class_sessions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.timeslot_id == id);
        return item ?? new timeslot();
    }

    // public async Task<timeslot> AddAsync(timeslot entity)
    // {
    //     _context.timeslots.Add(entity);
    //     await _context.SaveChangesAsync();
    //     return entity;
    // }
    //
    // public async Task UpdateAsync(timeslot entity)
    // {
    //     _context.Entry(entity).State = EntityState.Modified;
    //     await _context.SaveChangesAsync();
    // }
    //
    // public async Task<bool> DeleteAsync(int timeslotId)
    // {
    //     var item = await _context.timeslots.FindAsync(timeslotId);
    //
    //     if (item == null)
    //     {
    //         return false;
    //     }
    //     
    //     _context.timeslots.Remove(item);
    //     return await _context.SaveChangesAsync() > 0;
    // }

    public async Task<IEnumerable<timeslot>> SearchTimeslotsAsync(TimeOnly? startTime = null, TimeOnly? endTime = null)
    {
        IQueryable<timeslot> query = _dbSet;

        if (startTime.HasValue)
        {
            query = query.Where(t => t.start_time == startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(t => t.end_time == endTime.Value);
        }
        // Bạn có thể thêm các điều kiện tìm kiếm khác ở đây (ví dụ: start_time >= startTime.Value)
        // if (startTime.HasValue)
        // {
        //     query = query.Where(t => t.start_time >= startTime.Value);
        // }

        // if (endTime.HasValue)
        // {
        //     query = query.Where(t => t.end_time <= endTime.Value);
        // }


        return await query.ToListAsync();
    }
}