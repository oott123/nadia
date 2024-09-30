using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nadia.Presets;

class DetailedBsod : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"ControlSet001\Control\CrashControl",
            "DisplayParameters",
            "1"
        );
    }
}
