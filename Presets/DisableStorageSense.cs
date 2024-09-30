using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nadia.Presets;

public class DisableStorageSense : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy",
            "01",
            "0"
        );
    }
}
