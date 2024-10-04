using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nadia.Services;
using Newtonsoft.Json.Linq;

namespace nadia.Runners;

public abstract class BaseRunner
{
    public required OfflineRegistryProvider OfflineRegistryProvider { protected get; init; }
    public required MountProvider MountProvider { protected get; init; }

    protected OfflineRegistry Registry
    {
        get { return OfflineRegistryProvider.Registry!; }
    }

    protected string MountDir
    {
        get { return MountProvider.MountDir; }
    }

    public abstract Task Run(JObject? args);
}
