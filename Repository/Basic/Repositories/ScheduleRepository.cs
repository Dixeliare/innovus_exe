using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class ScheduleRepository : GenericRepository<schedule>, IScheduleRepository
{
    public ScheduleRepository(AppDbContext context) : base(context)
    {
        
    }

    public async Task<schedule?> GetByMonthYearExactAsync(DateOnly monthYear)
    {
        // So sánh trực tiếp DateOnly
        return await _dbSet
            .FirstOrDefaultAsync(s => s.month_year == monthYear);
    }

    public async Task<IEnumerable<schedule>> GetSchedulesInMonthYearAsync(int month, int year)
    {
        // Lọc theo Month và Year từ DateOnly
        return await _dbSet
            .Where(s => s.month_year.HasValue &&
                        s.month_year.Value.Month == month &&
                        s.month_year.Value.Year == year)
            // .Include(s => s.weeks) // Thêm nếu muốn eager load weeks
            .ToListAsync();
    }

    public async Task<IEnumerable<schedule>> SearchByIdOrNoteAsync(int? id, string? note)
    {
        IQueryable<schedule> query = _dbSet;

        if (id.HasValue)
        {
            query = query.Where(s => s.schedule_id == id.Value);
        }

        if (!string.IsNullOrEmpty(note))
        {
            query = query.Where(s => EF.Functions.ILike(s.note, $"%{note}%"));
        }
        // Thêm eager loading nếu cần hiển thị chi tiết weeks cho kết quả tìm kiếm
        // .Include(s => s.weeks)
        return await query.ToListAsync();
    }

}