using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Serilog;
using static nadia.WinUtilTweakService;

namespace nadia.Runners;

public record class ApplyWinUtilsArgs
{
    public required string Definition { get; init; }
    public required string[] Tweaks { get; init; }
}

public class ApplyWinUtils : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<ApplyWinUtilsArgs>();
        var findDirs = new string[]
        {
            Path.Join(Directory.GetCurrentDirectory(), "winutil"),
            Path.Join(Assembly.GetExecutingAssembly().Location, "..", "winutil"),
        };

        foreach (var dir in findDirs)
        {
            var file = Path.Join(dir, ap.Definition);
            if (File.Exists(file))
            {
                WinUtil.ApplyTweaks(file, ap.Tweaks, Registry);
                break;
            }
        }
    }
}
