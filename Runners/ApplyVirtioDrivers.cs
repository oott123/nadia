using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DiscUtils.Iso9660;
using DiscUtils.Udf;
using Microsoft.Dism;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public record class ApplyVirtioDriversArgs
{
    public required string Iso { get; init; }
}

public class ApplyVirtioDrivers : BaseRunner
{
    private string[] _systems =
    [
        "w11",
        "2k25",
        "w10",
        "2k22",
        "2k19",
        "2k16",
        "w8.1",
        "w8",
        "w7",
        "2k12R2",
        "2k12",
        "2k8R2",
        "2k8",
        "2k3",
        "xp",
    ];
    private string _arch = "amd64";

    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<ApplyVirtioDriversArgs>();

        using var file = File.OpenRead(ap.Iso);
        using var cd = new CDReader(file, false);

        var driversDir = "virtio-drivers";
        if (Directory.Exists(driversDir))
        {
            Directory.Delete(driversDir, true);
        }

        Directory.CreateDirectory(driversDir);

        var drivers = cd.GetDirectories("");

        foreach (var driverName in drivers)
        {
            foreach (var system in _systems)
            {
                var d = $"{driverName}\\{system}\\{_arch}";
                if (cd.DirectoryExists(d))
                {
                    Log.Information($"extracting virtio drivers: {d}");
                    var files = cd.GetFiles(d);
                    var fsDir = Path.Join(driversDir, driverName);
                    Directory.CreateDirectory(fsDir);
                    foreach (var filePathInIso in files)
                    {
                        if (filePathInIso.EndsWith(".pdb"))
                        {
                            continue;
                        }

                        using var srcStream = cd.OpenFile(filePathInIso, FileMode.Open);
                        using var destStream = File.Open(
                            Path.Join(fsDir, Path.GetFileName(filePathInIso)),
                            FileMode.Create
                        );
                        destStream.Seek(0, SeekOrigin.Begin);
                        await srcStream.CopyToAsync(destStream);
                    }
                    break;
                }
            }
        }

        Log.Information("adding drivers into image");
        using var session = DismApi.OpenOfflineSession(MountDir);
        DismApi.AddDriversEx(session, driversDir, true, true);
    }
}
