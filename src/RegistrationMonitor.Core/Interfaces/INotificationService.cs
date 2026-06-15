using RegistrationMonitor.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationMonitor.Core.Interfaces
{
    /// <summary>
    /// Service for notifications(Telegram / Email)
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Send "registration open" notification
        /// </summary>
        /// <param name="info"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task SendRegistrationOpenedAsync(RegistrationInfo info, CancellationToken token = default);
    }
}
