namespace DiscordPortable
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Portable;
    using SilDev;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Log.AllowLogging();
            bool newInstance;
            using (new Mutex(true, ProcessEx.CurrentName, out newInstance))
            {
                if (!newInstance)
                    return;

                var regKeys = new[]
                {
                    "HKCR\\Discord"
                };

                if (Elevation.IsAdministrator && Environment.CommandLine.ContainsEx("{E429830E-A5A1-4FA0-9D5F-B21964F4698E}"))
                    goto ClearRegistry;

                var updaterPath = PathEx.Combine(PathEx.LocalDir, "App\\Discord\\Update.exe");
                if (!File.Exists(updaterPath) || ProcessEx.IsRunning("Discord"))
                    return;

                var anyKeyExists = regKeys.Any(Reg.SubKeyExists);

                var dirMap = new Dictionary<string, string>
                {
                    {
                        "%LocalAppData%\\Discord",
                        "%CurDir%\\App\\Discord"
                    },
                    {
                        "%LocalAppData%\\SquirrelTemp",
                        "%CurDir%\\App\\Discord\\packages\\SquirrelTemp"
                    },
                    {
                        "%AppData%\\discord",
                        "%CurDir%\\Data\\AppData"
                    }
                };

                Helper.DirectoryForwarding(Helper.Options.Start, dirMap);

                Helper.ApplicationStart(updaterPath, "--processStart Discord.exe", false);

                Helper.DirectoryForwarding(Helper.Options.Exit, dirMap);

                if (anyKeyExists)
                    return;

                ClearRegistry:
                foreach (var key in regKeys)
                    Reg.RemoveSubKey(key);
                if (!Elevation.IsAdministrator && regKeys.Any(Reg.SubKeyExists))
                    Elevation.RestartAsAdministrator("{E429830E-A5A1-4FA0-9D5F-B21964F4698E}");
            }
        }
    }
}
