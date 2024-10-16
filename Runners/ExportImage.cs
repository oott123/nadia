using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public record class ExportImageArgs
{
    public required string OutWim { get; init; }
}

public class ExportImage : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<ExportImageArgs>();

        DismUtils.UnmountWim(MountDir, true);
        await DismUtils.RebuildImage(MountProvider.SourceWim, ap.OutWim, MountProvider.WimIndex);
        File.Delete(MountProvider.SourceWim);
    }
}
