using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace nadia.Presets;

public class DisablePagingFile : IPreset
{
    public required OfflineRegistry Registry;

    public void Run()
    {
        using var subkey = Registry.MachineSystem.RootKey.OpenSubKey(
            @"ControlSet001\Control\Session Manager\Memory Management"
        );
        if (subkey == null)
        {
            Log.Warning("no memory management key found");
            return;
        }

        subkey.SetValue("ExistingPageFiles", Array.Empty<string>());
        subkey.SetValue("PagingFiles", Array.Empty<string>());
    }
}
