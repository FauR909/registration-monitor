using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using RegistrationMonitor.Core.Entities;
using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationMonitor.Infrastructure.Repositories
{
    public sealed class StatusRepository : IStatusRepository
    {
        private readonly AppDbContext _context;

        private readonly ILogger<StatusRepository> _logger;

        public StatusRepository(AppDbContext context, ILogger<StatusRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<StatusCheckRecord?> GetLastCheckAsync(CancellationToken token = default) 
        {
            var lastCheck = await _context.StatusChecks
                .AsNoTracking()
                .OrderByDescending(r => r.CheckedAt)
                .FirstOrDefaultAsync(token);

            _logger.LogDebug("Last check: {Status} at {Time}",
                lastCheck?.DetectedStatus.ToString() ?? "Unknown",
                lastCheck?.CheckedAt.ToString("HH:mm:ss") ?? "-");

            return lastCheck;
        }

        public async Task AddCheckAsync(StatusCheckRecord record, CancellationToken token = default)
        {
            await _context.StatusChecks.AddAsync(record, token);
            await _context.SaveChangesAsync(token);

            _logger.LogDebug("New saved check: Id={Id}, Status={Status}, NotificationSent={Sent}",
                record.Id,
                record.DetectedStatus,
                record.NotificationSent);
        }
    }
}
