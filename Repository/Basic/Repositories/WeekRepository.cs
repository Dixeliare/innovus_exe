using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class WeekRepository : GenericRepository<week>
{
    public WeekRepository()
    {
    }

    public WeekRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<week>> GetAll()
    {
        var items = await _context.weeks
            .Include(c => c.class_sessions)
            .Include(s => s.schedule)
            .AsSplitQuery()
            .ToListAsync();
        return items ?? new List<week>();
    }

    public async Task<week> GetById(int id)
    {
        var item = await _context.weeks
            .Include(c => c.class_sessions)
            .Include(s => s.schedule)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.week_id == id);
        return item ?? new week();
    }

    public async Task<week> AddAsync(week entity)
    {
        _context.weeks.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(week entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.weeks.FindAsync(id);

        if (item == null)
        {
            return false;
        }
        
        _context.weeks.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<IEnumerable<week>> GetWeeksByScheduleIdAsync(int scheduleId)
    {
        return await _context.weeks
            .Where(w => w.schedule_id == scheduleId)
            .Include(w => w.schedule)
            .ToListAsync();
    }

    public async Task<IEnumerable<week>> SearchWeeksAsync(DateOnly? dayOfWeek = null, int? scheduleId = null)
    {
        IQueryable<week> query = _context.weeks;

        // Xây dựng điều kiện WHERE dựa trên các tham số được cung cấp.
        // Điều kiện sẽ là "day_of_week = @dayOfWeek OR schedule_id = @scheduleId"
        // Hoặc chỉ một trong hai nếu tham số kia là null.
        query = query.Where(w =>
                (dayOfWeek == null ||
                 w.day_of_week ==
                 dayOfWeek.Value) && // Nếu dayOfWeek là null, bỏ qua điều kiện này. Nếu không null, yêu cầu phải khớp.
                (scheduleId == null ||
                 w.schedule_id ==
                 scheduleId.Value) // Nếu scheduleId là null, bỏ qua điều kiện này. Nếu không null, yêu cầu phải khớp.
        );
        // Lưu ý: Nếu bạn muốn logic "OR" (hoặc cái này, hoặc cái kia, hoặc cả hai)
        // thì cấu trúc sẽ là:
        // query = query.Where(w =>
        //     (dayOfWeek.HasValue && w.day_of_week == dayOfWeek.Value) ||
        //     (scheduleId.HasValue && w.schedule_id == scheduleId.Value)
        // );
        // Tuy nhiên, logic trên (dùng AND giữa hai điều kiện con) là để đảm bảo
        // Nếu cả hai đều được cung cấp, thì nó sẽ tìm cả 2
        // Nếu chỉ 1 được cung cấp, nó tìm theo cái đó
        // Nếu cả 2 đều NULL, nó trả về tất cả
        // Đây là cách phổ biến cho chức năng search-filter.
        // Nếu bạn muốn "hoặc cái này, hoặc cái kia (ít nhất một)", hãy dùng khối OR ở trên.

        // Với yêu cầu "search theo schedule_id HOẶC day_of_week",
        // tôi sẽ đề xuất logic sau, nó sẽ trả về tuần nếu khớp với DAY OF WEEK HOẶC SCHEDULE ID.
        // Nếu cả hai đều null, nó sẽ trả về TẤT CẢ.
        // Nếu bạn muốn CHỈ tìm kiếm khi ít nhất một tham số được cung cấp:
        if (dayOfWeek.HasValue || scheduleId.HasValue)
        {
            query = query.Where(w =>
                (dayOfWeek.HasValue && w.day_of_week == dayOfWeek.Value) ||
                (scheduleId.HasValue && w.schedule_id == scheduleId.Value)
            );
        }
        // Nếu cả dayOfWeek và scheduleId đều là null, query sẽ không được lọc và trả về tất cả.

        // Bạn có thể thêm `.Include()` nếu muốn eager load các navigation properties
        // Ví dụ: .Include(w => w.schedule) nếu bạn muốn lấy thông tin schedule cùng lúc

        return await query.ToListAsync();
    }
}