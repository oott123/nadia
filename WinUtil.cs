using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using Serilog;

namespace nadia;

public record class WinUtilTweak
{
    public WinUtilTweakRegistry[]? registry { get; set; }
    public WinUtilTweakService[]? service { get; set; }
}

public record class WinUtilTweakRegistry
{
    public string? Path { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Value { get; set; }
}

public record class WinUtilTweakService
{
    public string? Name { get; set; }
    public string? StartupType { get; set; }

    public static class WinUtil
    {
        public static void ApplyTweaks(
            string configFile,
            string[] entries,
            OfflineRegistry registry
        )
        {
            var configJson = File.ReadAllText(configFile)
                // winutil uses new lines in there json files, which we don't support.
                // however, we don't even use any scripts, so it's save to strip all new lines.
                .Replace("\n", "");
            var tweaks = JsonSerializer.Deserialize<Dictionary<string, WinUtilTweak>>(configJson);
            if (tweaks == null)
            {
                throw new Exception("Failed to read winutil tweaks.json");
            }

            foreach (var entry in entries)
            {
                var tweak = tweaks[entry];
                if (tweak == null)
                {
                    Log.Warning($"no such tweak: {entry}");
                    continue;
                }

                var ok = false;

                if (tweak.registry != null && tweak.registry.Length >= 0)
                {
                    ok = true;
                    foreach (var registryEntry in tweak.registry)
                    {
                        string subPath;
                        Hive hive;

                        if (string.IsNullOrEmpty(registryEntry.Path))
                        {
                            continue;
                        }

                        var rPath = registryEntry.Path.ToLower();

                        if (rPath.StartsWith(@"hklm:\software\"))
                        {
                            hive = registry.MachineSoftware;
                            subPath = registryEntry.Path.Substring(@"HKLM:\SOFTWARE\".Length);
                        }
                        else if (rPath.StartsWith(@"hklm:\system\currentcontrolset\"))
                        {
                            hive = registry.MachineSystem;
                            subPath =
                                @"ControlSet001\"
                                + registryEntry.Path.Substring(
                                    @"HKLM:\System\CurrentControlSet\".Length
                                );
                        }
                        else if (rPath.StartsWith(@"hklm:\system\"))
                        {
                            hive = registry.MachineSystem;
                            subPath = registryEntry.Path.Substring(@"HKLM:\System\".Length);
                        }
                        else if (rPath.StartsWith(@"hkcu:\"))
                        {
                            hive = registry.NtUser;
                            subPath = registryEntry.Path.Substring(@"HKCU:\".Length);
                        }
                        else
                        {
                            Log.Warning(
                                $"unknown registry path in entry {entry}: {registryEntry.Path}"
                            );
                            continue;
                        }

                        RegistryValueKind kind;
                        if (registryEntry.Type == "DWord")
                        {
                            kind = RegistryValueKind.DWord;
                        }
                        else if (registryEntry.Type == "String")
                        {
                            kind = RegistryValueKind.String;
                        }
                        else
                        {
                            Log.Warning(
                                $"unkown registry type in entry {entry}: {registryEntry.Type}"
                            );
                            continue;
                        }

                        RegistryUtils.RegSetValue(
                            hive,
                            subPath,
                            registryEntry.Name,
                            registryEntry.Value,
                            kind
                        );
                    }
                }

                if (tweak.service != null && tweak.service.Length > 0)
                {
                    ok = true;
                    foreach (var item in tweak.service)
                    {
                        int type = 0;
                        if (item.StartupType == "Manual")
                        {
                            type = 3;
                        }
                        else if (item.StartupType == "Automatic")
                        {
                            type = 2;
                        }
                        else if (item.StartupType == "System")
                        {
                            type = 1;
                        }
                        else if (item.StartupType == "Disabled")
                        {
                            type = 4;
                        }
                        else if (item.StartupType == "Boot")
                        {
                            type = 0;
                        }
                        else if (item.StartupType == "AutomaticDelayedStart")
                        {
                            type = 6;
                        }
                        else
                        {
                            Log.Warning(
                                $"unknown service type for entry {entry} and service {item.Name}"
                            );
                            continue;
                        }

                        RegistryUtils.SetServiceStart(registry, item.Name, type);
                    }
                }

                if (!ok)
                {
                    Log.Warning($"{entry} is not appliable offline");
                }
            }
        }
    }
}
