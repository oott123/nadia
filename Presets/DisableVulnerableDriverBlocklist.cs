using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nadia.Presets;

public class DisableVulnerableDriverBlocklist : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"ControlSet001\Control\CI\Config",
            "VulnerableDriverBlocklistEnable",
            "0"
        );
    }
}
