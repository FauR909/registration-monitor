using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationMonitor.Core.Models
{
    /// <summary>
    /// Result of checking registration status
    /// </summary>
    /// <param name="Status">Registration status</param>
    /// <param name="SourceMessage">Message from parser(logging purpose)</param>
    /// <param name="OpenFrom">Date of registration opening</param>
    /// <param name="OpenUntil">Date of registration closing</param>
    public sealed record RegistrationInfo(
        
        RegistrationStatus Status,

        string? SourceMessage = null,

        DateTimeOffset? OpenFrom = null,
        
        DateTimeOffset? OpenUntil = null
    );
}
