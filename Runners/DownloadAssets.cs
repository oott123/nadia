using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace nadia.Runners;

public record class DownloadAssetsArgs
{
    public required AssetMeta[] Assets { get; init; }
}

public class DownloadAssets : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<DownloadAssetsArgs>();
        await DownloadUtils.DownloadAssets(ap.Assets);
    }
}
