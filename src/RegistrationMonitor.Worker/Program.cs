using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.VisualBasic;
using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Core.Models;
using RegistrationMonitor.Core.Services;
using RegistrationMonitor.Infrastructure;
using RegistrationMonitor.Infrastructure.Data;
using RegistrationMonitor.Infrastructure.Notifications;
using RegistrationMonitor.Infrastructure.Repositories;
using RegistrationMonitor.Infrastructure.Telegram;
using RegistrationMonitor.Infrastructure.Trackers;
using RegistrationMonitor.Worker;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.Configure<TelegramOptions>(
    builder.Configuration.GetSection(TelegramOptions.SectionName));

builder.Services.Configure<MonitoringOptions>(
    builder.Configuration.GetSection(MonitoringOptions.SectionName));

var telegramOptions = builder.Configuration
    .GetSection(TelegramOptions.SectionName)
    .Get<TelegramOptions>() ?? new TelegramOptions();

if (!telegramOptions.IsConfigured) 
{
    var isDevelopment = builder.Environment.IsDevelopment();

    if (isDevelopment)
    {
        Console.WriteLine(
            "WARNING: Telegram is not configured. " +
            "Using ConsoleNotificationService instead");
    }
    else 
    {
        throw new InvalidOperationException(
            "Configure ChatID and BotToken");
    }
}

// Database
builder.Services.AddDbContextFactory<AppDbContext>(options => 
{
    var connectionString = builder.Configuration
        .GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("'DefaultConnection' not found");

    var fileName = connectionString.Replace("Data Source=", string.Empty).Trim();
    var dbPath = Path.Combine(AppPaths.DataDirectory, fileName);

    options.UseSqlite($"Data Source={dbPath}");
});

// Repository
builder.Services.AddScoped<IStatusRepository, StatusRepository>();

// Dummy tracker instead of real site, while real system is unavailable
builder.Services.AddScoped<IRegistrationTracker, DummyRegistrationTracker>();

// Subscribers repository
builder.Services.AddScoped<ISubscriberRepository, SubscriberRepository>();

// Telegram client notification service
if (telegramOptions.IsConfigured)
{
    builder.Services.AddSingleton<ITelegramBotClient>(
        new TelegramBotClient(telegramOptions.BotToken));

    builder.Services.AddScoped<INotificationService, TelegramNotificationService>();

    builder.Services.AddHostedService<TelegramSubscriptionListener>();
}
else 
{
    // Fallback on Console if Telegram failed (Development only)
    builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();
}

// Orchestrator
builder.Services.AddScoped<MonitoringOrchestrator>();

// Configure shutdown timeout
builder.Services.Configure<HostOptions>(options => 
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(10);
});

// Main loop
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await ApplyMigrationsAsync(host.Services);
await host.RunAsync();


static async Task ApplyMigrationsAsync(IServiceProvider services) 
{
    await using var scope = services.CreateAsyncScope();

    var db = scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<AppDbContext>>()
        .CreateDbContext();

    var logger = scope.ServiceProvider
        .GetRequiredService<ILogger<Program>>();

    var pending = await db.Database.GetPendingMigrationsAsync();
    var pendingList = pending.ToList();

    if (pendingList.Count == 0) 
    {
        logger.LogInformation("Migrations already set");
        return;
    }

    logger.LogInformation(
        "Setting {Count} migrations: {Names}",
        pendingList.Count,
        string.Join(' ', pendingList));

    await db.Database.MigrateAsync();

    logger.LogInformation($"Migrations set successfully");
}
