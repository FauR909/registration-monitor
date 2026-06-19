using System;
using System.Collections.Generic;
using System.Text;

namespace RegistrationMonitor.Core.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        private static readonly TimeZoneInfo KyivTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

        public static DateTimeOffset ToKyivTime(this DateTimeOffset utcTime) =>
        TimeZoneInfo.ConvertTime(utcTime, KyivTimeZone);
    }
}
