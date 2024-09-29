﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Serilog;

namespace nadia.Presets;

public class RemoveWindowsDefender : IPreset
{
    public required string MountDir;
    public required OfflineRegistry Registry;

    public void Run()
    {
        Log.Information("removing windows defender files");
        FileUtils.TakeOwnAndDelete(Path.Join(MountDir, "Program Files", "Windows Defender"));
        FileUtils.TakeOwnAndDelete(
            Path.Join(MountDir, "Program Files", "Windows Defender Advanced Threat Protection")
        );
        FileUtils.TakeOwnAndDelete(Path.Join(MountDir, "Program Files (x86)", "Windows Defender"));
        FileUtils.TakeOwnAndDelete(
            Path.Join(
                MountDir,
                "Program Files (x86)",
                "Windows Defender Advanced Threat Protection"
            )
        );
        FileUtils.TakeOwnAndDelete(
            Path.Join(MountDir, "ProgramData", "Microsoft", "Windows Defender")
        );
        FileUtils.TakeOwnAndDelete(
            Path.Join(
                MountDir,
                "ProgramData",
                "Microsoft",
                "Windows Defender Advanced Threat Protection"
            )
        );

        Log.Information("disabling windows defender services");
        RegistryUtils.DisableService(Registry, "WinDefend");
        RegistryUtils.DisableService(Registry, "WdNisSvc");
        RegistryUtils.DisableService(Registry, "WdNisDrv");
        RegistryUtils.DisableService(Registry, "WdFilter");
        RegistryUtils.DisableService(Registry, "Sense");
    }
}
