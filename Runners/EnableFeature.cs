using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dism;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public record class EnableFeatureArgs
{
    public required string Name { get; init; }
}

public class EnableFeature : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<EnableFeatureArgs>();
        Log.Information($"feature name: ${ap.Name}");
        using var session = DismApi.OpenOfflineSession(MountDir);
        DismApi.EnableFeature(session, ap.Name, true, true);
    }
}
