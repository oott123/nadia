using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace nadia.Runners;

public record class ExtractWimArgs
{
    public required string Iso { get; init; }
    public required string Wim { get; init; }
}

public class ExtractWim : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<ExtractWimArgs>();

        await IsoUtils.ExtractFromIso(ap.Iso, "sources\\install.wim", ap.Wim);
    }
}
