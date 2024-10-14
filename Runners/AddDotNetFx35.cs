using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public record class AddDotNetFx35Args
{
    public required string Iso { get; init; }
}

public class AddDotNetFx35 : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<AddDotNetFx35Args>();

        Directory.CreateDirectory("sxs");

        var cab = "sxs\\Microsoft-Windows-NetFx3-OnDemand-Package~31bf3856ad364e35~amd64~~.cab";

        Log.Information("extract ondemand package");
        await IsoUtils.ExtractFromIso(
            ap.Iso,
            "sources\\sxs\\Microsoft-Windows-NetFx3-OnDemand-Package~31bf3856ad364e35~amd64~~.cab",
            cab
        );

        Log.Information("apply netfx package");
        DismUtils.AddPackages(MountDir, [cab]);
    }
}
