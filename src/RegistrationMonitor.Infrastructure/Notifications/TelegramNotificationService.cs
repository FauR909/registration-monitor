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
using Telegram.Bot.Exceptions;

namespace RegistrationMonitor.Infrastructure.Notifications
{
    /// <summary>
    /// Realisation INotificationService through Telegram Bot API
    /// </summary>
    public sealed class TelegramNotificationService : INotificationService
    {
        private readonly ILogger<TelegramNotificationService> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly ISubscriberRepository _subscriberRepository;

        public TelegramNotificationService(
            ITelegramBotClient botClient,
            ISubscriberRepository subscriberRepository,
            ILogger<TelegramNotificationService> logger)
        {
            _logger = logger;
            _subscriberRepository = subscriberRepository;
            _botClient = botClient;
        }

        public async Task SendRegistrationOpenedAsync(RegistrationInfo info, CancellationToken token = default) 
        {
            var subscribers = await _subscriberRepository.GetActiveSubscribersAsync(token);

            if (subscribers.Count == 0) 
            {
                _logger.LogWarning("There is no active subscribers. Notification not sent");
                throw new InvalidOperationException("No active subscribers to notify");
            }

            var message = BuildMessage(info);
            var successCount = 0;

            foreach (var subscriber in subscribers) 
            {
                try
                {
                    await _botClient.SendMessage(
                        subscriber.ChatId, message, parseMode: ParseMode.Html, cancellationToken: token);
                    successCount++;

                    await Task.Delay(50, token);
                }
                catch (ApiRequestException ex) when (ex.ErrorCode == 403)
                {
                    _logger.LogWarning("Subscriber blocked bot. Deactivated.");
                    await _subscriberRepository.DeactivateAsync(subscriber.ChatId, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification to {ChatId}", subscriber.ChatId);
                }
            }

            if (successCount == 0) 
            {
                throw new InvalidOperationException("Failed to notify any subscriber.");
            }

            _logger.LogInformation("Notification sent to {Success}/{Total} subscribers.",
                successCount, subscribers.Count);
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
