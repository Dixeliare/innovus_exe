using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class TimeslotRepository : GenericRepository<timeslot>
{
    public TimeslotRepository()
    {
    }
    
    public TimeslotRepository(AppDbContext context) => _context = context;

    public async Task<List<timeslot>> GetAllAsync()
    {
        var items = await _context.timeslots.Include(c => c.class_sessions).ToListAsync();
        return items ?? new List<timeslot>();
    }

    public async Task<timeslot> GetByIdAsync(int id)
    {
        var item = await _context.timeslots.Include(c => c.class_sessions).FirstOrDefaultAsync(c => c.timeslot_id == id);
        return item ?? new timeslot();
    }

    public async Task<int> CreateAsync(timeslot timeslot)
    {
        await _context.timeslots.AddAsync(timeslot);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(timeslot timeslot)
    {
        var item = await _context.timeslots.FindAsync(timeslot.timeslot_id);

        if (item == null)
        {
            return 0; //throw new NotFoundException("Timeslot not found");
        }
        
        item.start_time = timeslot.start_time;
        item.end_time = timeslot.end_time;

        return await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int timeslotId)
    {
        var item = await _context.timeslots.FindAsync(timeslotId);

        if (item == null)
        {
            return false;
        }
        
        _context.timeslots.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<timeslot>> SearchTimeslotsAsync(TimeOnly? startTime, TimeOnly? endTime)
    {
        IQueryable<timeslot> query = _context.timeslots;

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