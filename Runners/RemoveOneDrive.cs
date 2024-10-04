using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public class RemoveOneDrive : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        Log.Information("removing onedrive");
        FileUtils.TakeOwnAndDelete(Path.Join(MountDir, "Windows", "System32", "OneDriveSetup.exe"));
    }
}
