using System;
using System.Collections.Generic;
using System.Text;

namespace RegistrationMonitor.Core.Models
{
    /// <summary>
    /// TelegramBot configuration.
    /// </summary>
    public sealed class TelegramOptions
    {
        public const string SectionName = "Telegram";

        public string BotToken { get; init; } = string.Empty;

        public string ChatId { get; init; } = string.Empty;


        public bool IsConfigured => 
            !string.IsNullOrWhiteSpace(BotToken) &&
            !string.IsNullOrWhiteSpace(ChatId);
    }

    public sealed class MonitoringOptions 
    {
        public const string SectionName = "MonitoringOptions";

        public int CheckIntervalSeconds { get; init; } = 30;

        public TimeSpan CheckInterval => TimeSpan.FromSeconds(CheckIntervalSeconds);
    }
}
