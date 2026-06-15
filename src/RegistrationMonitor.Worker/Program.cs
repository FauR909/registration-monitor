using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Infrastructure.Trackers;
using RegistrationMonitor.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Dummy tracker instead of real site, while real system is unavailable
builder.Services.AddScoped<IRegistrationTracker, DummyRegistrationTracker>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
