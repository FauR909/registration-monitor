using System;
using System.Collections.Generic;
using System.Text;

namespace RegistrationMonitor.Core.Entities
{
    /// <summary>
    /// Class that represent one telegram bot subscriber. Mutable with /start, /stop
    /// </summary>
    public sealed class Subscriber
    {
        public int Id { get; init; }

        public long ChatId { get; init; }

        public string? Username { get; set; }

        public DateTimeOffset SubscribedAt { get; init; }

        public bool IsActive { get; set; }
    }
}
