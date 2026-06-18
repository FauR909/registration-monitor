using System;
using System.Collections.Generic;
using System.Text;

namespace RegistrationMonitor.Core.Models
{
    internal enum CheckAction
    {
        /// <summary>
        /// Status is the same as last check, no action needed. This is the default value.
        /// </summary>
        NoChange,

        /// <summary>
        /// Status has changed since last check, update database with new status. Notification may be needed based on the new status.
        /// </summary>
        StatusChanged,

        /// <summary>
        /// Status has changed to open since last check, but notification has not been sent yet. Update database and send notification.
        /// </summary>
        NotificationPending
    }
}
