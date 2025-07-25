using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class DayOfWeekLookupRepository : GenericRepository<day_of_week_lookup>, IDayOfWeekLookupRepository
{
    public DayOfWeekLookupRepository(AppDbContext context) : base(context)
    {
    }

    // Có thể ghi đè hoặc thêm các phương thức cụ thể nếu cần,
    // nhưng GenericRepository thường đủ cho các thao tác cơ bản của lookup table.
    // Ví dụ, để GetAll có thể bao gồm các navigation property nếu có (nhưng lookup table thường không có).
    public async Task<IEnumerable<day_of_week_lookup>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
}