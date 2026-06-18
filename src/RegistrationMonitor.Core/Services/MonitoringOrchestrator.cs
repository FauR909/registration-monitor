using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using RegistrationMonitor.Core.Entities;
using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Core.Models;

namespace RegistrationMonitor.Core.Services
{   /// <summary>
    /// Represents the orchestrator for monitoring operations.
    /// Mainly responsible for coordinating the monitoring process, including fetching data, 
    /// processing it, and handling any necessary actions based on the monitoring results.
    /// <summary>
    public sealed class MonitoringOrchestrator
    {
        private readonly ILogger<MonitoringOrchestrator> _logger;
        private readonly IRegistrationTracker _tracker;
        private readonly IStatusRepository _repository;
        private readonly INotificationService _notification;

        public MonitoringOrchestrator(
            ILogger<MonitoringOrchestrator> logger,
            IRegistrationTracker tracker,
            IStatusRepository repository,
            INotificationService notification)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
        }

        public async Task<OrchestratorResult> RunCheckAsync(CancellationToken token = default) 
        {
            RegistrationInfo currentInfo;

            try
            {
                currentInfo = await _tracker.GetCurrentStatusAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current ststus");

                return OrchestratorResult.Failed($"Tracker error: {ex.Message}");
            }

            _logger.LogInformation("Current status: {Status}, SourceMessage: {SourceMessage}",
                currentInfo.Status, currentInfo.SourceMessage);

            var lastRecord = await _repository.GetLastCheckAsync(token);

            var action = DecideAction(currentInfo, lastRecord);

            _logger.LogDebug("Decided action: {Action}", action);

            return action switch
            {
                CheckAction.NoChange => HandleNoChange(currentInfo),
                CheckAction.StatusChanged => await HandleStatusChangedAsync(currentInfo, lastRecord, token),
                CheckAction.NotificationPending => await HandleNotificationPendingAsync(currentInfo, lastRecord!, token),
                _ => OrchestratorResult.Skipped("Uknown action")
            };

        }
        /// <summary>
        /// Cases:
        /// NoChane: Current status is the same as the last detected status.
        /// StatusChanged: Current status is different from the last detected status.
        /// NotificationPending: Current status is Open, and no notification has been sent yet.
        /// </summary>
        private static CheckAction DecideAction(
            RegistrationInfo current,
            StatusCheckRecord? last) 
        {
            if (last is null)
            {
                return CheckAction.StatusChanged;
            }

            if (last.DetectedStatus != current.Status)
            {
                return CheckAction.StatusChanged;
            }

            if (current.Status == RegistrationStatus.Open && !last.NotificationSent) 
            {
                return CheckAction.NotificationPending;
            }

            return CheckAction.NoChange;
        }


        private OrchestratorResult HandleNoChange(RegistrationInfo current) 
        {
            _logger.LogInformation("No change detected. Current status: {Status}",
                current.Status);

            return OrchestratorResult.Skipped($"No change detected: {current.Status}");
        }

        private async Task<OrchestratorResult> HandleStatusChangedAsync(
            RegistrationInfo current,
            StatusCheckRecord? last,
            CancellationToken token) 
        {
            _logger.LogWarning("Status changed from {Old} to {New}"
                , last?.DetectedStatus.ToString() ?? "Unknown"
                , current.Status);

            var notificationSent = false;

            if (current.Status == RegistrationStatus.Open) 
            {
                notificationSent = await TrySendNotificationAsync(current, token);
            }

            var record = new StatusCheckRecord
            {
                CheckedAt = DateTimeOffset.UtcNow,
                DetectedStatus = current.Status,
                NotificationSent = notificationSent,
                SourceMessage = current.SourceMessage
            };

            await _repository.AddCheckAsync(record, token);

            return OrchestratorResult.Processed(
                current.Status, notificationSent,
                $"Status changed. Notification {(notificationSent ? "sent" : "not sent")}");
        }

        private async Task<OrchestratorResult> HandleNotificationPendingAsync(
            RegistrationInfo current,
            StatusCheckRecord existingRecord,
            CancellationToken token) 
        {
            _logger.LogWarning("Retry sending notification. Id={Id}", existingRecord.Id);

            var sent = await TrySendNotificationAsync(current, token);

            if (sent)
            {
                var updatedRecord = new StatusCheckRecord
                {
                    CheckedAt = DateTimeOffset.UtcNow,
                    DetectedStatus = current.Status,
                    NotificationSent = true,
                    SourceMessage = "[Retry] " + current.SourceMessage
                };

                await _repository.AddCheckAsync(updatedRecord, token);

                _logger.LogInformation("Notification sent after retry");
            }

            return OrchestratorResult.Processed(
                current.Status, sent,
                $"Notification retry {(sent ? "succeeded" : "failed")}");
        }

        private async Task<bool> TrySendNotificationAsync(
            RegistrationInfo info,
            CancellationToken token) 
        {
            try
            {
                await _notification.SendRegistrationOpenedAsync(info, token);
                _logger.LogInformation("Notification sent!");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification. Try again with next check.");
                return false;
                throw;
            }
        }

    }
}
