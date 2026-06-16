using Microsoft.EntityFrameworkCore;
using RegistrationMonitor.Core.Entities;
using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Core.Models;
using RegistrationMonitor.Infrastructure.Data;

namespace RegistrationMonitor.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RegistrationMonitor started. Interval between checks: {Interval} sec.", _checkInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Checking... [{Time}]", DateTimeOffset.UtcNow.ToString("HH:mm:ss"));
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var tracker = scope.ServiceProvider.GetRequiredService<IRegistrationTracker>();

                var repository = scope.ServiceProvider.GetRequiredService<IStatusRepository>();

                var result = await tracker.GetCurrentStatusAsync(stoppingToken);

                var lastRecord = await repository.GetLastCheckAsync(stoppingToken);

                var statusChanged = lastRecord is null || lastRecord.DetectedStatus != result.Status;

                if (!statusChanged)
                {
                    _logger.LogInformation(
                        "Status has not changed, skip db. Status: {Status}",
                        result.Status);
                }
                else
                {
                    var newRecord = new StatusCheckRecord
                    {
                        CheckedAt = DateTimeOffset.UtcNow,
                        DetectedStatus = result.Status,
                        NotificationSent = false,
                        SourceMessage = result.SourceMessage,
                    };

                    await repository.AddCheckAsync(newRecord, stoppingToken);

                    _logger.LogWarning(
                        "Status changed from {Old} to {New}. Saved to db (Id = {Id})",
                        lastRecord?.DetectedStatus.ToString() ?? "No data",
                        result.Status,
                        newRecord.Id);

                    if (result.Status == RegistrationStatus.Open) 
                    {
                        _logger.LogWarning("Registration opened");
                    }
                }

                //LogResult(result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error while checking.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("RegistrationMonitor stopped.");
    }

    private void LogResult(RegistrationInfo info) 
    {
        switch (info.Status) 
        {
            case RegistrationStatus.Open:
                _logger.LogWarning("OPEN! Info: {Message}", info.SourceMessage);
                break;
            case RegistrationStatus.Closed:
                _logger.LogInformation("CLOSED! Info: {Message}", info.SourceMessage);
                break;
            default:
                _logger.LogWarning("UNKNOWN!. Info: {Message}", info.SourceMessage);
                break;
        }
    }
}
