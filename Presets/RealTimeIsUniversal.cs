using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nadia.Presets;

public class RealTimeIsUniversal : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"ControlSet001\Control\TimeZoneInformation",
            "RealTimeIsUniversal",
            "1"
        );
    }
}
