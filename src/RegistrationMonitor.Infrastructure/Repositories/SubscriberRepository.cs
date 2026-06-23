using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using RegistrationMonitor.Core.Entities;
using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.PortableExecutable;
using System.Text;

namespace RegistrationMonitor.Infrastructure.Repositories
{
    public sealed class SubscriberRepository : ISubscriberRepository
    {
        private readonly AppDbContext _context;

        private readonly ILogger<SubscriberRepository> _logger;

        public SubscriberRepository(AppDbContext context, ILogger<SubscriberRepository> logger) 
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Subscriber>> GetActiveSubscribersAsync(CancellationToken token = default) 
        {
            return await _context.Subscribers
                .AsNoTracking()
                .Where(s => s.IsActive)
                .ToListAsync(token);
        }

        public async Task AddOrActivateAsync(long chatId, string? username, CancellationToken token)
        {
            var existing = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.ChatId == chatId, token);

            if(existing is not null)
            {
                existing.IsActive = true;
                existing.Username = username;
                _logger.LogInformation("Subscriber {ChatId} reactivated.", chatId);
            }
            else
            {
                _context.Subscribers.Add(new Subscriber
                {
                    ChatId = chatId,
                    Username = username,
                    IsActive = true,
                    SubscribedAt = DateTimeOffset.UtcNow
                });
                _logger.LogInformation("New subscriber: {ChatId}", chatId);
            }

            await _context.SaveChangesAsync(token);
        }

        public async Task DeactivateAsync(long chatId, CancellationToken token) 
        {
            var existing = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.ChatId == chatId, token);

            if (existing is null) 
            {
                return;
            }

            existing.IsActive = false;

            await _context.SaveChangesAsync(token);

            _logger.LogInformation("Subscriber {ChatId} deactivated", chatId);
        }
    }
}
