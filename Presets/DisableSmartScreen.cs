using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nadia.Presets;

public class DisableSmartScreen : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\System",
            "EnableSmartScreen",
            "0"
        );
    }
}
