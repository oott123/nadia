using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public record class ApplyUpdatesArgs
{
    public required string Dir { get; init; }
}

public class ApplyUpdates : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<ApplyUpdatesArgs>();
        var updateDir = ap.Dir;

        if (Directory.Exists(updateDir))
        {
            Log.Information("installing updates");
            var updateFiles = Directory.EnumerateFiles(
                updateDir,
                "*.msu",
                new EnumerationOptions { RecurseSubdirectories = true }
            );
            DismUtils.AddPackages(MountDir, updateFiles.ToArray());
        }
        else
        {
            Log.Information("unable to find updates directory, skipping");
        }
    }
}
