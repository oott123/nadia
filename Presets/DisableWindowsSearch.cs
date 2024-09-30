using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nadia.Presets;

public class DisableWindowsSearch : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        RegistryUtils.DisableService(Registry, "WSearch");
    }
}
