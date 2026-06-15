using RegistrationMonitor.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationMonitor.Core.Interfaces
{
    public interface IRegistrationTracker
    {
        /// <summary>
        /// Asynchronously checks registration status
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns><see cref="RegistrationInfo"/> with current status</returns>
        Task<RegistrationInfo> GetCurrentStatusAsync(CancellationToken token = default);
    }
}
