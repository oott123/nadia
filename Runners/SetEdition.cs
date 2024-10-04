using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public record class SetEditionArgs
{
    public required string Sku { get; init; }
}

public class SetEdition : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<SetEditionArgs>();
        await DismUtils.SetEdition(MountDir, ap.Sku);
    }
}
