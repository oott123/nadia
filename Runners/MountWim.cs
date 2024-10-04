using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Util;
using Microsoft.Dism;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public record class MountWimArgs
{
    public required string Wim { get; init; }
    public required string Image { get; init; }
}

public class MountWim : BaseRunner, IDisposable
{
    private string _wim;
    private string _dir;
    private int _index;

    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<MountWimArgs>();

        var wimImages = DismApi.GetImageInfo(ap.Wim);
        foreach (var image in wimImages)
        {
            Log.Information($"Image #{image.ImageIndex}: {image.ImageName}");
        }
        var wimImage = wimImages.Where((i) => i.ImageName == ap.Image).First();
        var wimIndex = wimImage.ImageIndex;

        _dir = @"build\windows";
        _wim = ap.Wim;
        _index = wimIndex;

        MountProvider.MountDir = _dir;
        MountProvider.SourceWim = _wim;
        MountProvider.WimIndex = wimIndex;

        DismUtils.MountWim(_wim, _dir, wimIndex);
    }

    public void Dispose()
    {
        var images = DismApi.GetMountedImages();
        var wimFull = Path.GetFullPath(_wim);
        var destFull = Path.GetFullPath(_dir);
        foreach (var imageInfo in images)
        {
            if (imageInfo.MountPath == destFull)
            {
                if (imageInfo.ImageFilePath == wimFull && imageInfo.ImageIndex == _index)
                {
                    Log.Information("Image still mounted, cleaning up...");
                    DismUtils.UnmountWim(destFull, false);
                    return;
                }
            }
        }
    }
}
