using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace nadia.Runners;

public class MountRegistry : BaseRunner, IDisposable
{
    private OfflineRegistry _registry;

    public override async Task Run(JObject? args)
    {
        _registry = OfflineRegistry.MountRegistry(MountDir);
        OfflineRegistryProvider.Registry = _registry;
    }

    public void Dispose()
    {
        _registry.SaveRegistry();
    }
}
