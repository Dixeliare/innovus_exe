using Repository.Basic.Repositories;
using Repository.Data;
using Repository.Models;
using Services.IServices;

namespace Services.Services;

public class ScheduleService : IScheduleService
{
    private readonly ScheduleRepository _scheduleRepository;
    // Số tháng lịch trình cần duy trì trong tương lai
    private const int MONTHS_TO_ENSURE_IN_FUTURE = 6;
    // Số tháng cũ cần giữ lại (ví dụ: 3 tháng cũ + tháng hiện tại)
    private const int MONTHS_TO_KEEP_OLD = 3;
    
    
    public ScheduleService(ScheduleRepository scheduleRepository) => _scheduleRepository = scheduleRepository;
    
    public async Task<List<schedule>> GetAllAsync()
    {
        return await _scheduleRepository.GetAllAsync();
    }

    public async Task<schedule> GetByIDAsync(int id)
    {
        return await _scheduleRepository.GetByIDAsync(id);
    }

    public async Task<List<schedule>> SearchByIdOrNoteAsync(int? id, string? note)
    {
        return await _scheduleRepository.SearchByIdOrNoteAsync(id, note);
    }

    public async Task<List<schedule>> SearchByMonthYearAsync(int? month, int? year)
    {
        return await _scheduleRepository.SearchByMonthYearAsync(month, year);
    }

    public async Task<int> CreateSchedule(schedule schedule)
    {
        return await _scheduleRepository.CreateAsync(schedule);
    }

    public async Task<int> UpdateSchedule(schedule schedule)
    {
        return await _scheduleRepository.UpdateSchedule(schedule);
    }

    public async Task<bool> DeleteAsync(int scheduleId)
    {
        return await _scheduleRepository.DeleteAsync(scheduleId);
    }

    
    // ---------- Logic Chính cho việc tạo và dọn dẹp Schedule tự động ----------
    
    public async Task EnsureScheduleExistenceAndCleanupAsync()
        {
            var now = DateTime.UtcNow; // Sử dụng UTC để tránh vấn đề múi giờ
            var currentMonthStart = new DateOnly(now.Year, now.Month, 1);

            Console.WriteLine($"[ScheduleService] Running EnsureScheduleExistenceAndCleanupAsync at {now:yyyy-MM-dd HH:mm:ss}");

            // Bước 1: Đảm bảo các Schedule cho tương lai
            await EnsureFutureSchedulesAsync(currentMonthStart);

            // Bước 2: Dọn dẹp các Schedule cũ
            await CleanupOldSchedulesAsync(currentMonthStart);

            Console.WriteLine($"[ScheduleService] Finished EnsureScheduleExistenceAndCleanupAsync.");
        }

        private async Task EnsureFutureSchedulesAsync(DateOnly currentMonthStart)
        {
            Console.WriteLine($"[ScheduleService] Ensuring schedules for next {MONTHS_TO_ENSURE_IN_FUTURE} months...");

            var schedulesToAdd = new List<schedule>();

            for (int i = 0; i < MONTHS_TO_ENSURE_IN_FUTURE; i++)
            {
                var targetDate = currentMonthStart.AddMonths(i);
                // Tạo một DateOnly mới với ngày là 1 để đảm bảo tính nhất quán
                var targetMonthYear = new DateOnly(targetDate.Year, targetDate.Month, 1);

                // Kiểm tra xem schedule cho tháng/năm này đã tồn tại chưa
                var existingSchedule = await _scheduleRepository.SearchByMonthYearAsync(targetMonthYear.Month, targetMonthYear.Year);

                if (existingSchedule == null || !existingSchedule.Any())
                {
                    // Nếu chưa có, thêm vào danh sách cần tạo
                    schedulesToAdd.Add(new schedule
                    {
                        month_year = targetMonthYear,
                        note = $"Lịch trình tháng {targetMonthYear.Month}/{targetMonthYear.Year}"
                        // Các thuộc tính mặc định khác nếu có
                    });
                    Console.WriteLine($"[ScheduleService] - Preparing to create schedule for {targetMonthYear.Month}/{targetMonthYear.Year}.");
                }
            }

            if (schedulesToAdd.Any())
            {
                // Sử dụng AddRangeAsync nếu bạn có trong GenericRepository
                // Hoặc lặp qua và gọi CreateSchedule từng cái một
                foreach (var s in schedulesToAdd)
                {
                    await _scheduleRepository.CreateSchedule(s);
                }
                Console.WriteLine($"[ScheduleService] - Created {schedulesToAdd.Count} new schedules.");
            }
            else
            {
                Console.WriteLine("[ScheduleService] - All future schedules already exist.");
            }
        }

        private async Task CleanupOldSchedulesAsync(DateOnly currentMonthStart)
        {
            Console.WriteLine($"[ScheduleService] Cleaning up schedules older than {MONTHS_TO_KEEP_OLD} months...");

            // Xác định mốc thời gian cũ cần xóa
            // Ví dụ: tháng hiện tại là tháng 4, muốn xóa từ tháng 12 trở về trước (4 - 3 = tháng 1).
            // Tức là những schedule có month_year <= (currentMonthStart - 3 tháng) sẽ bị xóa.
            var cutoffDate = currentMonthStart.AddMonths(-MONTHS_TO_KEEP_OLD);

            // Tìm tất cả các schedule cũ hơn mốc thời gian đã định
            // Lưu ý: DbContext.RemoveRange cần các thực thể được theo dõi (tracked entities)
            var oldSchedules = await _scheduleRepository.GetAllAsync(); // Lấy tất cả để lọc trên bộ nhớ (hoặc dùng FindAsync nếu có nhiều bản ghi)
            oldSchedules = oldSchedules
                .Where(s => s.month_year.HasValue && s.month_year.Value < cutoffDate)
                .ToList();

            if (oldSchedules.Any())
            {
                Console.WriteLine($"[ScheduleService] - Found {oldSchedules.Count} old schedules to delete before {cutoffDate.Month}/{cutoffDate.Year}.");
                foreach (var s in oldSchedules)
                {
                    await _scheduleRepository.DeleteAsync(s.schedule_id); // Gọi hàm DeleteAsync của repo
                }
                Console.WriteLine($"[ScheduleService] - Deleted {oldSchedules.Count} old schedules.");
            }
            else
            {
                Console.WriteLine("[ScheduleService] - No old schedules to delete.");
            }
        }
}