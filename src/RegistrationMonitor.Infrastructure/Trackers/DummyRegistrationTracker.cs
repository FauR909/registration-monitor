using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Core.Models;

namespace RegistrationMonitor.Infrastructure.Trackers
{
    public class DummyRegistrationTracker : IRegistrationTracker
    {
        private readonly ILogger<DummyRegistrationTracker> _logger;

        private static readonly DateTimeOffset SimulatedOpenTime = DateTimeOffset.UtcNow.AddMinutes(2);

        public DummyRegistrationTracker(ILogger<DummyRegistrationTracker> logger) 
        {
            _logger = logger;
        }

        public Task<RegistrationInfo> GetCurrentStatusAsync(CancellationToken token) 
        {
            var now = DateTimeOffset.UtcNow;
            var isOpen = now >= SimulatedOpenTime;

            _logger.LogDebug("[DummyTracker] Check simulation. Time: {Now:HH:mm:ss}." + 
                "Registration opens at: {OpenTime:HH:mm:ss}. Status: {Status}",
                now, SimulatedOpenTime, isOpen ? "OPEN" : "CLOSED");

            var info = new RegistrationInfo(
                Status: isOpen ? RegistrationStatus.Open : RegistrationStatus.Closed,
                SourceMessage: isOpen ? 
                    $"[Simulation] Registration opened at {SimulatedOpenTime:HH:mm:ss} UTC" : 
                    $"[Simulation] Waiting. Registration will be open at {SimulatedOpenTime:HH:mm:ss} UTC",
                OpenFrom: isOpen ? SimulatedOpenTime : null );

            return Task.FromResult(info);
        }
    }
}
