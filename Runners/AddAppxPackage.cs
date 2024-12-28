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

public record class AddAppxPackageArgs
{
    public required string File { get; init; }
}

public class AddAppxPackage : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<AddAppxPackageArgs>();
        Log.Information($"adding appx package {ap.File} into image");
        using var session = DismApi.OpenOfflineSession(MountDir);
        DismApi.AddProvisionedAppxPackage(session, ap.File, null, null, "");
    }
}
