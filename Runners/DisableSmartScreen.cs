using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace nadia.Runners;

public class DisableSmartScreen : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\System",
            "EnableSmartScreen",
            "0"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows\CurrentVersion\Run\SecurityHealth"
        );
    }
}
