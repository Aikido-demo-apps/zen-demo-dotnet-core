namespace zen_demo_dotnet.Helpers
{
    public sealed class StoredSsrfBackgroundService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

        private readonly AppHelpers _appHelpers;
        private readonly ILogger<StoredSsrfBackgroundService> _logger;

        public StoredSsrfBackgroundService(
            AppHelpers appHelpers,
            ILogger<StoredSsrfBackgroundService> logger)
        {
            _appHelpers = appHelpers;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await _appHelpers.ProcessStoredSsrfQueueAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Stored SSRF background service is stopping.");
            }
        }
    }
}
