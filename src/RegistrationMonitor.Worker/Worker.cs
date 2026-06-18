using Microsoft.EntityFrameworkCore;
using RegistrationMonitor.Core.Entities;
using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Core.Models;
using RegistrationMonitor.Infrastructure.Data;
using RegistrationMonitor.Core.Services;

namespace RegistrationMonitor.Worker;

/// <summary>
/// Now worker only responsible for planning. Business-logic is handled by MonitoringOrchestrator.
/// </summary>
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

                var orchestrator = scope.ServiceProvider.GetRequiredService<MonitoringOrchestrator>();

                var result = await orchestrator.RunCheckAsync(stoppingToken);

                if (!result.IsSuccess)
                {
                    _logger.LogError("Check failed. Message: {Message}", result.Message);
                }
                else if (result.WasProcessed)
                {
                    _logger.LogWarning("Processed: {}", result.Message);
                }
                else 
                {
                    _logger.LogInformation("{Msg}", result.Message);
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
