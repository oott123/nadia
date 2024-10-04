using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public class RemoveEdge : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        Log.Information("removing edge files");
        FileUtils.TakeOwnAndDelete(Path.Join(MountDir, "Program Files (x86)", "Microsoft", "Edge"));
        FileUtils.TakeOwnAndDelete(
            Path.Join(MountDir, "Program Files (x86)", "Microsoft", "EdgeUpdate")
        );
        FileUtils.TakeOwnAndDelete(
            Path.Join(MountDir, "Program Files (x86)", "Microsoft", "EdgeCore")
        );

        Log.Information("removing edge webview files");
        var sxs = Directory.GetDirectories(Path.Join(MountDir, "Windows", "WinSxS"));
        foreach (var item in sxs.Where((i) => i.Contains("_microsoft-edge-webview_")))
        {
            FileUtils.TakeOwnAndDelete(item);
        }
        FileUtils.TakeOwnAndDelete(
            Path.Join(MountDir, "Windows", "System32", "Microsoft-Edge-Webview")
        );

        Log.Information("removing edge webview registry");
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge Update"
        );
    }
}
