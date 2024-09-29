using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace nadia.Presets
{
    class SkipFirstLogonAnimation : IPreset
    {
        public required OfflineRegistry Registry;

        public void Run()
        {
            RegistryUtils.RegSetValue(
                Registry.MachineSoftware,
                @"Microsoft\Windows\CurrentVersion\Policies\System",
                "EnableFirstLogonAnimation",
                "0"
            );
            RegistryUtils.RegSetValue(
                Registry.MachineSoftware,
                @"Microsoft\Windows\CurrentVersion\Policies\System",
                "EnableFirstLogonAnimation",
                "0"
            );
        }
    }
}
