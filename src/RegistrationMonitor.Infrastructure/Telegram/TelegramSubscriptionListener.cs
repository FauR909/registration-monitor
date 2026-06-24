using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using RegistrationMonitor.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RegistrationMonitor.Infrastructure.Telegram
{
    /// <summary>
    /// Background service for listening inbox messages from Telegram via long polling. 
    /// Handles /start and /stop.
    /// </summary>
    public sealed class TelegramSubscriptionListener : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TelegramSubscriptionListener> _logger;

        public TelegramSubscriptionListener(
            ITelegramBotClient client,
            IServiceScopeFactory scopeFactory,
            ILogger<TelegramSubscriptionListener> logger)
        {
            _botClient = client;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message }
            };

            _logger.LogInformation("Telegram listener started");

            await _botClient.ReceiveAsync(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: token);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token) 
        {
            if (update.Message is not { Text: { } messageText } message) 
            {
                return;
            }

            var chatId = message.Chat.Id;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var subscribers = scope.ServiceProvider.GetRequiredService<ISubscriberRepository>();

            switch (messageText.Trim().ToLowerInvariant()) 
            {
                case ("/start"):
                    await subscribers.AddOrActivateAsync(chatId, message.Chat.Username, token);
                    await botClient.SendMessage(
                        chatId,
                        "✅ Success! You will receive a notification as soon as registration opens.\n\n" +
                        "Send /stop to cancel subscription",
                        cancellationToken: token);
                    _logger.LogInformation("New subscriber: {ChatId}", chatId);
                    break;

                case ("/stop"):
                    await subscribers.DeactivateAsync(chatId, token);
                    await botClient.SendMessage(
                        chatId,
                        "You unsubscribed successfully. To resubscribe - send /start",
                        cancellationToken: token);
                    _logger.LogInformation("Unsubscribed: {ChatId}", chatId);
                    break;

                default:
                    await botClient.SendMessage(
                        chatId,
                        "👋 Hello! I will send a notification as soon as registration opens.\n\n" +
                        "/start - subscribe\n/stop - unsubscribe",
                        cancellationToken: token);
                    break;

            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token) 
        {
            _logger.LogError(exception, "Error while listening Telegram-updates.");
            return Task.CompletedTask;
        }
    }
}
