using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace nadia.Presets;

public class CleanupXboxGameBar : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR",
            "AppCaptureEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"System\GameConfigStore",
            "GameDVR_Enabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\GameBar",
            "ShowStartupPanel",
            "0"
        );

        foreach (var item in new string[] { "ms-gamebar", "ms-gamebarservices" })
        {
            using var subkey =
                Registry.MachineSoftware.RootKey.OpenSubKey($"Classes\\{item}")
                ?? Registry.MachineSoftware.RootKey.CreateSubKey($"Classes\\{item}");
            subkey.SetValue("", $"URL:{item}");
            subkey.SetValue("URL Protocol", " ");
            subkey.SetValue("NoOpenWith", " ");
            using var openSubKey =
                subkey.OpenSubKey(@"shell\open\command")
                ?? subkey.CreateSubKey(@"shell\open\command");
            openSubKey.SetValue("", @"\System32\systray.exe");
        }
    }
}
