using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace nadia.Runners;

public class CleanupXboxGameBar : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\GameDVR",
            "AllowGameDVR",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Microsoft\WindowsRuntime\ActivatableClassId\Windows.Gaming.GameBar.Prese nceServer.Internal.PresenceWriter",
            "ActivationType",
            "0"
        );
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
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\GameBar",
            "UseNexusForGameBarEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\GameBar",
            "ShowGameModeNotifications",
            "0"
        );

        foreach (var item in new string[] { "ms-gamebar", "ms-gamebarservices" })
        {
            using var subkey =
                Registry.MachineSoftware.RootKey.OpenSubKey($"Classes\\{item}", true)
                ?? Registry.MachineSoftware.RootKey.CreateSubKey($"Classes\\{item}");
            subkey.SetValue("", $"URL:{item}");
            subkey.SetValue("URL Protocol", " ");
            subkey.SetValue("NoOpenWith", " ");
            using var openSubKey =
                subkey.OpenSubKey(@"shell\open\command", true)
                ?? subkey.CreateSubKey(@"shell\open\command");
            openSubKey.SetValue("", @"\System32\systray.exe");
        }

        RegistryUtils.DisableService(Registry, "xbgm");
        RegistryUtils.DisableService(Registry, "XboxGipSvc");
        RegistryUtils.DisableService(Registry, "XblAuthManager");
        RegistryUtils.DisableService(Registry, "XblGameSave");
        RegistryUtils.DisableService(Registry, "XboxNetApiSvc");
    }
}
