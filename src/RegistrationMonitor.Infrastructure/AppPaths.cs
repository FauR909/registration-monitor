using System;
using System.Collections.Generic;
using System.Text;

namespace RegistrationMonitor.Infrastructure
{
    public static class AppPaths
    {
        public static string DataDirectory { get; }

        static AppPaths()
        {
            DataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(DataDirectory);
        }
    }
}
