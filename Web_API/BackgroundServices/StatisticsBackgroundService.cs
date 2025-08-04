using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.IServices;

namespace Web_API.BackgroundServices
{
    public class StatisticsBackgroundService : BackgroundService
    {
        private readonly ILogger<StatisticsBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromHours(24); // Chạy mỗi 24 giờ

        public StatisticsBackgroundService(
            ILogger<StatisticsBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Statistics Background Service đã được khởi động.");

            using var timer = new PeriodicTimer(_period);
            
            // Chạy ngay lập tức lần đầu
            await UpdateStatisticsAsync();

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await UpdateStatisticsAsync();
            }
        }

        private async Task UpdateStatisticsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var statisticService = scope.ServiceProvider.GetRequiredService<IStatisticService>();

                _logger.LogInformation("Bắt đầu cập nhật thống kê tự động...");
                
                await statisticService.UpdateStatisticsAsync();
                
                _logger.LogInformation("Cập nhật thống kê tự động hoàn thành thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi xảy ra khi cập nhật thống kê tự động: {Message}", ex.Message);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Statistics Background Service đã được dừng.");
            await base.StopAsync(cancellationToken);
        }
    }
} 