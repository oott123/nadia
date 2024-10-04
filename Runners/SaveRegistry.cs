using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace nadia.Runners;

public class SaveRegistry : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        Registry.SaveRegistry();
    }
}
