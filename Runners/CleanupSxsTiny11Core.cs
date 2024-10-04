using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public class CleanupSxsTiny11Core : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var src = Path.Join(MountDir, "Windows", "WinSxS");
        var srcBackup = Path.Join(MountDir, "Windows", "WinSxS_back");
        var dest = Path.Join(MountDir, "Windows", "WinSxS_new");

        if (Directory.Exists(dest))
        {
            Log.Information("cleaning up last unsuccessful winsxs edit");
            Directory.Delete(dest, true);
        }

        if (Directory.Exists(srcBackup))
        {
            Log.Information("restoreing last unsuccessful winsxs delete");
            Directory.Delete(src, true);
            Directory.Move(srcBackup, src);
        }

        Log.Information("making new winsxs");
        Directory.CreateDirectory(dest);

        var prefixes = new string[]
        {
            "x86_microsoft.windows.common-controls_6595b64144ccf1df",
            "x86_microsoft.windows.gdiplus_6595b64144ccf1df",
            "x86_microsoft.windows.i..utomation.proxystub_6595b64144ccf1df",
            "x86_microsoft.windows.isolationautomation_6595b64144ccf1df",
            "x86_microsoft-windows-s..ngstack-onecorebase_31bf3856ad364e35",
            "x86_microsoft-windows-s..stack-termsrv-extra_31bf3856ad364e35",
            "x86_microsoft-windows-servicingstack_31bf3856ad364e35",
            "x86_microsoft-windows-servicingstack-inetsrv",
            "x86_microsoft-windows-servicingstack-onecore",
            "x86_microsoft.vc80.crt_1fc8b3b9a1e18e3b",
            "x86_microsoft.vc90.crt_1fc8b3b9a1e18e3b",
            "x86_microsoft.windows.c..-controls.resources_6595b64144ccf1df",
            "amd64_microsoft.vc80.crt_1fc8b3b9a1e18e3b",
            "amd64_microsoft.vc90.crt_1fc8b3b9a1e18e3b",
            "amd64_microsoft.windows.c..-controls.resources_6595b64144ccf1df",
            "amd64_microsoft.windows.common-controls_6595b64144ccf1df",
            "amd64_microsoft.windows.gdiplus_6595b64144ccf1df",
            "amd64_microsoft.windows.i..utomation.proxystub_6595b64144ccf1df",
            "amd64_microsoft.windows.isolationautomation_6595b64144ccf1df",
            "amd64_microsoft-windows-s..stack-inetsrv-extra_31bf3856ad364e35",
            "amd64_microsoft-windows-s..stack-msg.resources_31bf3856ad364e35",
            "amd64_microsoft-windows-s..stack-termsrv-extra_31bf3856ad364e35",
            "amd64_microsoft-windows-servicingstack_31bf3856ad364e35",
            "amd64_microsoft-windows-servicingstack-inetsrv_31bf3856ad364e35",
            "amd64_microsoft-windows-servicingstack-msg_31bf3856ad364e35",
            "amd64_microsoft-windows-servicingstack-onecore_31bf3856ad364e35",
            "Catalogs",
            "FileMaps",
            "Fusion",
            "InstallTemp",
            "Manifests",
        };

        var dirs = Directory
            .EnumerateDirectories(src)
            .Where((i) => prefixes.Any((z) => i.StartsWith(Path.Join(src, z))));

        foreach (var dir in dirs)
        {
            var entries = new Stack<string>();
            entries.Push(dir);

            while (entries.Count > 0)
            {
                var entry = entries.Pop();
                var destEntry = entry.Replace(src, dest);

                if (Directory.Exists(entry))
                {
                    Directory.CreateDirectory(destEntry);
                    foreach (var item in Directory.EnumerateFileSystemEntries(entry))
                    {
                        entries.Push(item);
                    }
                }
                else if (File.Exists(entry))
                {
                    if (File.Exists(destEntry))
                    {
                        File.Delete(destEntry);
                    }
                    File.Copy(entry, destEntry);
                }
                else
                {
                    Log.Warning($"sxs: {entry} is not exists");
                }
            }
        }

        Directory.Move(src, srcBackup);
        Directory.Move(dest, src);
        FileUtils.TakeOwnAndDelete(srcBackup);

        Log.Information("cleaning up image");
    }
}
