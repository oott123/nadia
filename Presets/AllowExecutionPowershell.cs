using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nadia.Presets;

public class AllowExecutionPowershell : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell",
            "ExecutionPolicy",
            "Unrestricted",
            Microsoft.Win32.RegistryValueKind.String
        );
    }
}
