using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace nadia.Runners;

public class Pause : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        Console.Write("Press enter to continue...");
        Console.ReadLine();
    }
}
