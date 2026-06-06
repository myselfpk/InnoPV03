using InnoPV.Web.Settings;
using Microsoft.Extensions.Options;

namespace InnoPV.Web.Services.SlaAlerts;

public class SlaAlertHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SlaAlertHostedService> _logger;
    private readonly SlaAlertSettings _settings;

    private DateTime? _lastRunDate;

    public SlaAlertHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<SlaAlertHostedService> logger,
        IOptions<SlaAlertSettings> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.EnableAutomaticAlerts)
        {
            _logger.LogInformation("Automatic SLA alerts are disabled.");
            return;
        }

        var intervalMinutes = _settings.CheckIntervalMinutes <= 0
            ? 30
            : _settings.CheckIntervalMinutes;

        await RunAlertsIfDueAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunAlertsIfDueAsync(stoppingToken);
        }
    }

    private async Task RunAlertsIfDueAsync(CancellationToken stoppingToken)
    {
        try
        {
            var now = DateTime.Now;
            var scheduledTime = GetScheduledTime(now);

            if (now < scheduledTime)
            {
                return;
            }

            if (_lastRunDate.HasValue && _lastRunDate.Value.Date == now.Date)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();

            var slaAlertService = scope.ServiceProvider.GetRequiredService<ISlaAlertService>();
            var result = await slaAlertService.SendAllAlertsAsync();

            _lastRunDate = now.Date;

            _logger.LogInformation(
                "Automatic SLA alerts processed on {Date}. Due soon: {DueSoonCaseCount}, Overdue: {OverdueCaseCount}, Recipients: {EmailRecipientCount}.",
                now,
                result.DueSoonCaseCount,
                result.OverdueCaseCount,
                result.EmailRecipientCount);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Automatic SLA alert processing failed.");
        }
    }

    private DateTime GetScheduledTime(DateTime now)
    {
        var hour = Math.Clamp(_settings.DailyRunHour, 0, 23);
        var minute = Math.Clamp(_settings.DailyRunMinute, 0, 59);

        return new DateTime(
            now.Year,
            now.Month,
            now.Day,
            hour,
            minute,
            0);
    }
}
