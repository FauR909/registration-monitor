using System;
using System.Collections.Generic;
using System.Text;
using TimeZoneConverter;

namespace RegistrationMonitor.Core.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        private static readonly TimeZoneInfo KyivTimeZone =
                    TZConvert.GetTimeZoneInfo("Europe/Kyiv");

        public static DateTimeOffset ToKyivTime(this DateTimeOffset utcTime) =>
            TimeZoneInfo.ConvertTime(utcTime, KyivTimeZone);
    }
}
