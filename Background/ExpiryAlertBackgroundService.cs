using MedTrack.Services;

namespace MedTrack.Background
{
    public class ExpiryAlertBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ExpiryAlertBackgroundService> _logger;

        public ExpiryAlertBackgroundService(IServiceProvider services, ILogger<ExpiryAlertBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expiry Alert Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

                    var created = await alertService.ScanAndCreateExpiryAlertsAsync();
                    if (created > 0)
                        _logger.LogInformation("Created {Count} new expiry alerts.", created);

                    var shortages = await alertService.DetectShortagesAsync();
                    if (shortages > 0)
                        _logger.LogInformation("Detected {Count} regional shortages.", shortages);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in expiry alert background service.");
                }

                // Run every 6 hours
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
    }
}
