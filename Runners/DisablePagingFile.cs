using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public class DisablePagingFile : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        using var subkey = Registry.MachineSystem.RootKey.OpenSubKey(
            @"ControlSet001\Control\Session Manager\Memory Management",
            true
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
