using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationMonitor.Core.Models
{
    public enum RegistrationStatus 
    {
        /// <summary>
        /// Unknown state(site unavailable / parsing error)
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Registration closed
        /// </summary>
        Closed = 1,

        /// <summary>
        /// Registration open
        /// </summary>
        Open = 2,
    }
}
