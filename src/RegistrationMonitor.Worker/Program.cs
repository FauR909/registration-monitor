using Microsoft.EntityFrameworkCore;
using RegistrationMonitor.Infrastructure.Data;
using RegistrationMonitor.Infrastructure.Repositories;
using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Infrastructure.Trackers;
using RegistrationMonitor.Worker;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.VisualBasic;

var builder = Host.CreateApplicationBuilder(args);

// Database
builder.Services.AddDbContextFactory<AppDbContext>(options => 
{
    var connectionString = builder.Configuration
        .GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("'DefaultConnection' not found");

    options.UseSqlite(connectionString);
});

// Repository
builder.Services.AddScoped<IStatusRepository, StatusRepository>();

// Dummy tracker instead of real site, while real system is unavailable
builder.Services.AddScoped<IRegistrationTracker, DummyRegistrationTracker>();

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
