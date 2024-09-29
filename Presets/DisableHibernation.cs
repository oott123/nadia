using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace nadia.Presets;

public class DisableSwapFile : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"ControlSet001\Control\Session Manager\Memory Management",
            "SwapfileControl",
            "0"
        );
    }
}
