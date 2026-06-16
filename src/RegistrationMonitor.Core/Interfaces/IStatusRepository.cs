using RegistrationMonitor.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationMonitor.Core.Interfaces
{
    public interface IStatusRepository
    {
        /// <summary>
        /// Get last check from db. Anti-spam mechanism
        /// </summary>
        /// <param name="token"></param>
        Task<StatusCheckRecord?> GetLastCheckAsync(CancellationToken token = default);

        /// <summary>
        /// Saves new check in db
        /// </summary>
        /// <param name="record"></param>
        /// <param name="token"></param>
        Task AddCheckAsync(StatusCheckRecord record, CancellationToken token = default);
    }
}
