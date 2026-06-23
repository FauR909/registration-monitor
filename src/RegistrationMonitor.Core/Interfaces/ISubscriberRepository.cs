using RegistrationMonitor.Core.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace RegistrationMonitor.Core.Interfaces
{
    public interface ISubscriberRepository
    {
        Task<IReadOnlyList<Subscriber>> GetActiveSubscribersAsync(CancellationToken token = default);

        Task AddOrActivateAsync(long chatId, string? username, CancellationToken token = default);

        Task DeactivateAsync(long chatId, CancellationToken token = default);
    }
}
