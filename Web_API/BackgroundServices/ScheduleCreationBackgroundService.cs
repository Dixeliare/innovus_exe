using Services.IServices;

namespace Web_API.BackgroundServices;

public class ScheduleCreationBackgroundService : BackgroundService
    {
        private readonly ILogger<ScheduleCreationBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private DateTime _lastRunDate = DateTime.MinValue; // Biến để theo dõi lần chạy cuối

        public ScheduleCreationBackgroundService(
            ILogger<ScheduleCreationBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Schedule Management Background Service is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("Schedule Management Background Service is stopping."));

            // Chạy ngay khi khởi động
            await RunScheduleManagementOnce(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Chờ đợi đến đầu ngày tiếp theo (hoặc bạn có thể chọn một thời điểm cụ thể)
                // Đây là cách đơn giản để đảm bảo nó chạy ít nhất một lần mỗi ngày
                var nextRunTime = DateTime.UtcNow.AddDays(1).Date.AddHours(2); // Ví dụ: 2 giờ sáng UTC
                var delayTime = nextRunTime - DateTime.UtcNow;
                if (delayTime < TimeSpan.Zero) delayTime = TimeSpan.FromSeconds(5); // Nếu đã qua giờ, chạy sau 5s

                _logger.LogInformation($"Next schedule management run at: {nextRunTime:yyyy-MM-dd HH:mm:ss} UTC (in {delayTime.TotalHours:F1} hours)");

                try
                {
                    await Task.Delay(delayTime, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Schedule Management Background Service delay cancelled.");
                    break; // Thoát vòng lặp nếu bị hủy trong khi chờ
                }

                // Thực thi logic chính
                if (!stoppingToken.IsCancellationRequested)
                {
                    await RunScheduleManagementOnce(stoppingToken);
                }
            }

            _logger.LogInformation("Schedule Management Background Service has stopped.");
        }

        private async Task RunScheduleManagementOnce(CancellationToken stoppingToken)
        {
            // Kiểm tra chỉ chạy một lần mỗi ngày
            if (_lastRunDate.Date == DateTime.UtcNow.Date)
            {
                _logger.LogInformation("Schedule Management: Already ran today. Skipping.");
                return;
            }

            _logger.LogInformation("Schedule Management Background Service is executing management logic.");

            using (var scope = _serviceProvider.CreateScope())
            {
                var scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();
                try
                {
                    await scheduleService.EnsureScheduleExistenceAndCleanupAsync();
                    _lastRunDate = DateTime.UtcNow; // Cập nhật thời gian chạy cuối cùng
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during schedule management.");
                }
            }
        }
    }