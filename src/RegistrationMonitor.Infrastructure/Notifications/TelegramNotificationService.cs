using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Core.Models;
using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types;
using Microsoft.EntityFrameworkCore.Storage.Json;
using RegistrationMonitor.Core.Extensions;

namespace RegistrationMonitor.Infrastructure.Notifications
{
    /// <summary>
    /// Realisation INotificationService through Telegram Bot API
    /// </summary>
    public sealed class TelegramNotificationService : INotificationService
    {
        private readonly ILogger<TelegramNotificationService> _logger;
        private readonly TelegramOptions _options;
        private readonly ITelegramBotClient _botClient;

        public TelegramNotificationService(
            ITelegramBotClient botClient,
            IOptions<TelegramOptions> options,
            ILogger<TelegramNotificationService> logger)
        {
            _logger = logger;
            _options = options.Value;
            _botClient = botClient;
        }

        public async Task SendRegistrationOpenedAsync(RegistrationInfo info, CancellationToken token = default) 
        {
            var message = BuildMessage(info);

            _logger.LogInformation("Sening Telegram-notification to ChatID: {ChatId}",
                _options.ChatId);

            await _botClient.SendMessage(
                chatId: _options.ChatId,
                text: message,
                parseMode: ParseMode.Html,
                cancellationToken: token);

            _logger.LogInformation("Notification sent successfully");
        }

        /// <summary>
        /// Forming HTML-notification for Telegram
        /// </summary>
        private static string BuildMessage(RegistrationInfo info) 
        {
            var lines = new List<string>
            {
                "<b>Registration opened!</b>", 
                string.Empty
            };

            if (info.OpenFrom.HasValue) 
            {
                var kyivOpenFrom = info.OpenFrom.Value.ToKyivTime();
                lines.Add($"<b>Opened from {kyivOpenFrom:dd.MM.yyyy HH:mm:ss} (Kyiv)</b>");
            }

            if (info.OpenUntil.HasValue) 
            {
                var kyivOpenUntil = info.OpenUntil.Value.ToKyivTime();
                lines.Add($"<b>Open until {kyivOpenUntil:dd.MM.yyyy HH:mm:ss} (Kyiv)</b>");
            }

            if (!string.IsNullOrWhiteSpace(info.SourceMessage)) 
            {
                lines.Add(string.Empty);
                lines.Add($"Info: {info.SourceMessage}");
            }

            var kyivNow = DateTimeOffset.UtcNow.ToKyivTime();
            lines.Add(string.Empty);
            lines.Add($"<i>Checked at {kyivNow:HH:mm:ss} (Kyiv)</i>");

            return string.Join("\n", lines);
        }

    }
}
