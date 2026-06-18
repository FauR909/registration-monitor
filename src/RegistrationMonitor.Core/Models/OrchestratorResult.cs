using System;
using System.Collections.Generic;
using System.Text;

namespace RegistrationMonitor.Core.Models
{

    /// <summary>
    /// Result of single check cycle by orchestrator. 
    /// It indicates whether the operation was successful, whether it was processed, and if a notification was sent.
    /// Used for logging and testing.
    /// </summary>
    public class OrchestratorResult
    {
        public bool IsSuccess { get; init; }

        public bool WasProcessed { get; init; }

        public bool NotificationSent { get; init; }

        public RegistrationStatus? DetectedStatus { get; init; }

        public string? Message { get; init; } = string.Empty;

        public static OrchestratorResult Skipped(string message) => new()
        {
            IsSuccess = true,
            WasProcessed = false,
            Message = message
        };

        public static OrchestratorResult Processed(RegistrationStatus status, bool notificationSent, string message) => new()
        {
            IsSuccess = true,
            WasProcessed = true,
            DetectedStatus = status,
            NotificationSent = notificationSent,
            Message = message
        };

        public static OrchestratorResult Failed(string message) => new()
        {
            IsSuccess = false,
            WasProcessed = false,
            Message = message
        };
    }
}
