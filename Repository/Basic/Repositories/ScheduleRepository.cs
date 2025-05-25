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

    public async Task<List<schedule>> SearchByIdOrNoteAsync(int? id, string? note) // Thay đổi tham số thành nullable
    {
        var query = _context.schedules
            .Include(w => w.weeks)
            .Include(u => u.user) // Giữ nguyên việc include user
            .AsQueryable(); // Bắt đầu xây dựng query

        // Kiểm tra xem có bất kỳ tiêu chí tìm kiếm nào được cung cấp không
        bool hasId = id.HasValue && id.Value > 0;
        bool hasNote = !string.IsNullOrEmpty(note);

        if (hasId && hasNote)
        {
            // Nếu cả ID và Note đều được cung cấp, tìm những bản ghi thỏa mãn ID HOẶC Note
            query = query.Where(s => s.schedule_id == id.Value || s.note.Contains(note));
        }
        else if (hasId)
        {
            // Chỉ tìm theo ID
            query = query.Where(s => s.schedule_id == id.Value);
        }
        else if (hasNote)
        {
            // Chỉ tìm theo Note
            query = query.Where(s => s.note.Contains(note));
        }
        else
        {
            // Nếu không có ID và Note nào được cung cấp, trả về tất cả.
            // Không cần thêm `.Where` nào vào query.
            // query = query; // Không cần dòng này vì query đã là AsQueryable() mặc định
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