using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegistrationMonitor.Core.Models;

namespace RegistrationMonitor.Core.Entities
{
    /// <summary>
    /// Represents a record of a status check, including the detected status, timestamp, and notification details.
    /// </summary>
    public sealed class StatusCheckRecord
    {

        public int Id { get; init; }

        /// <summary>
        /// When last check occur(UTC)
        /// </summary>
        public DateTimeOffset CheckedAt { get; init; }
        
        /// <summary>
        /// Status detected after last check
        /// </summary>
        public RegistrationStatus DetectedStatus { get; init; }

        /// <summary>
        /// Gets a value indicating whether a notification has been sent.
        /// </summary>
        public bool NotificationSent { get; init; }

        /// <summary>
        /// Message from tracker
        /// </summary>
        public string? SourceMessage { get; init; }
    }
}
