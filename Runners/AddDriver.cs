using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DiscUtils.Iso9660;
using DiscUtils.Udf;
using Microsoft.Dism;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public record class AddDriverArgs
{
    public required string Inf { get; init; }
}

public class AddDriver : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<AddDriverArgs>();
        Log.Information($"adding driver {ap.Inf} into image");
        using var session = DismApi.OpenOfflineSession(MountDir);
        DismApi.AddDriver(session, ap.Inf, true);
    }
}
