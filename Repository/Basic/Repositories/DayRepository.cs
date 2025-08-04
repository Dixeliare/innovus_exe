using Microsoft.EntityFrameworkCore;
using Repository.Basic.IRepositories;
using Repository.Data;
using Repository.Models;

namespace Repository.Basic.Repositories;

public class DayRepository : GenericRepository<day>, IDayRepository
    {
        // _context đã được khởi tạo trong GenericRepository
        public DayRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<day>> GetDaysByWeekIdAsync(int weekId)
        {
            return await _dbSet.Where(d => d.week_id == weekId).ToListAsync();
        }

        public async Task<IEnumerable<day>> GetDaysByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _dbSet.Where(d => d.date_of_day >= startDate && d.date_of_day <= endDate).ToListAsync();
        }

        public async Task<day?> GetDayWithClassSessionsAsync(int dayId)
        {
            return await _dbSet
                .Include(d => d.class_sessions) // Eager load class sessions
                    .ThenInclude(cs => cs.room) // Include room for class sessions
                .Include(d => d.class_sessions) // Re-include to branch for class
                    .ThenInclude(cs => cs._class) // Include class for class sessions
                .Include(d => d.class_sessions) // Re-include to branch for time slot
                    .ThenInclude(cs => cs.time_slot) // Include time slot for class sessions
                .FirstOrDefaultAsync(d => d.day_id == dayId);
        }

        public async Task<IEnumerable<day>> GetAllWithClassSessionsAsync()
        {
            return await _dbSet
                .Include(d => d.week) // Include week information
                .Include(d => d.class_sessions) // Include class sessions
                    .ThenInclude(cs => cs.room) // Include room for class sessions
                .Include(d => d.class_sessions) // Re-include to branch for class
                    .ThenInclude(cs => cs._class) // Include class for class sessions
                .Include(d => d.class_sessions) // Re-include to branch for time slot
                    .ThenInclude(cs => cs.time_slot) // Include time slot for class sessions
                .ToListAsync();
        }

        // TRIỂN KHAI PHƯƠNG THỨC NÀY:
        public async Task<IEnumerable<day>> SearchDaysAsync(DateOnly? dateOfDay = null, int? weekId = null, string? dayOfWeekName = null)
        {
            IQueryable<day> query = _dbSet;

            if (dateOfDay.HasValue)
            {
                query = query.Where(d => d.date_of_day == dateOfDay.Value);
            }

            if (weekId.HasValue)
            {
                query = query.Where(d => d.week_id == weekId.Value);
            }

            if (!string.IsNullOrWhiteSpace(dayOfWeekName))
            {
                // So sánh không phân biệt chữ hoa chữ thường
                query = query.Where(d => d.day_of_week_name != null && d.day_of_week_name.ToLower() == dayOfWeekName.ToLower());
            }
        
            // Eager load related Week and ClassSessions if needed in DTO mapping or subsequent operations
            // Đây là nơi bạn sẽ thêm .Include() nếu DayDto cần Week hoặc ClassSessions
            query = query
                .Include(d => d.week) // Bao gồm thông tin Week nếu cần cho DayDto
                .Include(d => d.class_sessions) // Bao gồm ClassSessions nếu cần cho DayDto
                    .ThenInclude(cs => cs.room); // Include room for class sessions

            return await query.ToListAsync();
        }
    }