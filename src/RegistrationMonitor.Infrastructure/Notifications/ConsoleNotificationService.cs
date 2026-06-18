using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistrationMonitor.Infrastructure.Notifications
{
    /// <summary>
    /// Dummy notification service that logging in console, used for testing or when no notification is needed.
    /// Will me replaced by real notification service like TelegramNotificationService in production.
    /// </summary>
    public sealed class ConsoleNotificationService : INotificationService
    {
        private readonly ILogger<ConsoleNotificationService> _logger;

        public ConsoleNotificationService(ILogger<ConsoleNotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendRegistrationOpenedAsync(
            RegistrationInfo info,
            CancellationToken token = default) 
        {
            _logger.LogWarning("[Console notification] Registration opened! Status: {Status}, Open from: {From}, Message: {Message})",
                info.Status,
                info.OpenFrom?.ToString("dd:MM:yyyy HH:mm:ss") ?? "Unknown",
                info.SourceMessage
                );

            return Task.CompletedTask;
        }
    }
}
