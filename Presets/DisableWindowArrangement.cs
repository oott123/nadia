using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace nadia.Presets;

public class DisableWindowArrangement : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Control Panel\Desktop",
            "WindowArrangementActive",
            "0"
        );
    }
}
