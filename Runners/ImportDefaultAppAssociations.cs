using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;
using static nadia.WinUtilTweakService;

namespace nadia.Runners;

public record class ImportDefaultAppAssociationsArgs
{
    public required string Xml { get; init; }
}

public class ImportDefaultAppAssociations : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<ImportDefaultAppAssociationsArgs>();
        var findDirs = new string[]
        {
            Path.Join(Directory.GetCurrentDirectory(), "associations"),
            Path.Join(Assembly.GetExecutingAssembly().Location, "..", "associations"),
        };

        foreach (var dir in findDirs)
        {
            var file = Path.Join(dir, ap.Xml);
            if (File.Exists(file))
            {
                await DismUtils.ImportDefaultAppAssociations(MountDir, file);
                break;
            }
        }
    }
}
