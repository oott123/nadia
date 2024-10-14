using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public class Pause : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
        {
            Log.Information("pause is skipped due to CI environment vairable is set");
        }
        else
        {
            Console.Write("Press enter to continue...");
            Console.ReadLine();
        }
    }
}
