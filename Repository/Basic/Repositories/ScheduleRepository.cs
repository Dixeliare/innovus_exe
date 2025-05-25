using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class ScheduleRepository : GenericRepository<schedule>
{
    public ScheduleRepository()
    {
    }

    public ScheduleRepository(AppDbContext context) => _context = context;

    public async Task<List<schedule>> GetAllAsync()
    {
        var items = await _context.schedules.Include(w => w.weeks).Include(u => u.user ).ToListAsync();
        return items ?? new List<schedule>();
    }

    public async Task<schedule> GetByIDAsync(int id)
    {
        var item = await _context.schedules.Include(w => w.weeks).Include(u => u.user).FirstOrDefaultAsync(s => s.schedule_id == id);
        return item ?? new schedule();
    }

    public async Task<List<schedule>> SearchByIdOrNoteAsync(int? id, string? note)
    {
        var query = _context.schedules
            .Include(w => w.weeks)
            .Include(u => u.user) 
            .AsQueryable(); // Bắt đầu xây dựng query

        if (id.HasValue && id.Value > 0)
        {
            query = query.Where(s => s.schedule_id == id.Value);
        }

        if (!string.IsNullOrEmpty(note))
        {
            query = query.Where(s => s.note.Contains(note));
        }

        var searchResult = await query.ToListAsync();

        return searchResult ?? new List<schedule>();
    }
    
    public async Task<List<schedule>> SearchByMonthYearAsync(int? month, int? year)
    {
        var query = _context.schedules
            .Include(w => w.weeks)
            .Include(u => u.user)
            .AsQueryable(); // Bắt đầu xây dựng query

        // Lọc theo Month và Year
        // Áp dụng điều kiện nếu cả month và year đều có giá trị hợp lệ
        if (month.HasValue && month.Value >= 1 && month.Value <= 12 &&
            year.HasValue && year.Value >= 1900)
        {
            query = query.Where(s => s.month_year.HasValue &&
                                     s.month_year.Value.Month == month.Value &&
                                     s.month_year.Value.Year == year.Value);
        }
        else if (month.HasValue && month.Value >= 1 && month.Value <= 12)
        {
            // Chỉ lọc theo tháng nếu năm không được cung cấp hoặc không hợp lệ
            query = query.Where(s => s.month_year.HasValue &&
                                     s.month_year.Value.Month == month.Value);
        }
        else if (year.HasValue && year.Value >= 1900)
        {
            // Chỉ lọc theo năm nếu tháng không được cung cấp hoặc không hợp lệ
            query = query.Where(s => s.month_year.HasValue &&
                                     s.month_year.Value.Year == year.Value);
        }

        var searchResult = await query.ToListAsync();

        return searchResult ?? new List<schedule>();
    }

    public async Task<int> CreateSchedule(schedule schedule)
    {
         await _context.schedules.AddAsync(schedule);
         return await _context.SaveChangesAsync();
    }

    public async Task<int> UpdateSchedule(schedule schedule)
    {
        var item = await _context.schedules.FindAsync(schedule.schedule_id);

        if (item != null)
        {
            return 0;
        }
        
        item.note = schedule.note;
        
        return await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int scheduleId)
    {
        var item = await _context.schedules.FindAsync(scheduleId);
        _context.schedules.Remove(item);
        
        int recordsAffected = await _context.SaveChangesAsync();
        return recordsAffected > 0;
    }
}