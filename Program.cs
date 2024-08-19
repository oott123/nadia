using DiscUtils.Iso9660;
using DiscUtils.Udf;
using Microsoft.Dism;
using nadia;
using Serilog;
using ShellProgressBar;

namespace Nadia;

public record class AssetMeta
{
    public required string[] Url { get; init; }
    public required string FileName { get; init; }
    public required string Hash { get; init; }
}

public record class DownloadRecord
{
    public required string FileName { get; init; }
    public required string Gid { get; init; }
    public required ChildProgressBar Bar { get; init; }
}

class Program
{
    static void Main(string[] args)
    {
        _Main(args).Wait();
    }

    static async Task _Main(string[] args)
    {
        var assets = new AssetMeta[]
        {
            new()
            {
                Url =
                [
                    "https://drive.massgrave.dev/26100.1.240331-1435.ge_release_CLIENT_ENTERPRISES_OEM_x64FRE_en-us.iso",
                ],
                FileName = "windows11_ltsc_26100.1.iso",
                Hash = "aaa4bd3254c1af5f9ce07f50db68fdead7a305878f2425c059ecd6b062a855b3",
            },
            new()
            {
                Url =
                [
                    "https://drive.massgrave.dev/26100.1.240331-1435.ge_release_amd64fre_CLIENT_LOF_PACKAGES_OEM.iso",
                ],
                FileName = "windows11_ltsc_client_lof_26100.1.iso",
                Hash = "fdbd87c2cd69ba84ef2ea69d5b468938355d0d634b7de7a1988480f94713a738",
            },
            new()
            {
                Url =
                [
                    "https://fedorapeople.org/groups/virt/virtio-win/direct-downloads/archive-virtio/virtio-win-0.1.262-2/virtio-win-0.1.262.iso",
                    "https://files.atto.town/d/mnt/box/virtio-win-0.1.262.iso?sign=71vv7dNtmyz-IDi-vY9Vkq29YEMbz32QDapc6TakfPM=:0",
                ],
                FileName = "virtio-win-0.1.262.iso",
                Hash = "bdc2ad1727a08b6d8a59d40e112d930f53a2b354bdef85903abaad896214f0a3",
            },
            new()
            {
                Url =
                [
                    "https://drive.massgrave.dev/26100.1.240331-1435.ge_release_amd64fre_ADK.iso",
                    "https://software-static.download.prss.microsoft.com/dbazure/888969d5-f34g-4e03-ac9d-1f9786c66749/26100.1.240331-1435.ge_release_amd64fre_ADK.iso",
                ],
                FileName = "adk_26100.1.iso",
                Hash = "d67308d386e37169b0a357ccbf2fded1a362dbd9550322ea866c439bd83f995d",
            },
            new()
            {
                Url =
                [
                    "https://drive.massgrave.dev/26100.1.240331-1435.ge_release_amd64fre_adkwinpeaddons.iso",
                    "https://software-static.download.prss.microsoft.com/dbazure/888969d5-f34g-4e03-ac9d-1f9786c66749/26100.1.240331-1435.ge_release_amd64fre_adkwinpeaddons.iso",
                ],
                FileName = "adk_winpeaddons_26100.1.iso",
                Hash = "c1688bf226face0f36d37282b7c276b3a8288856f8ae9d33518b5d9b445657fd",
            },
        };

        Log.Logger = new LoggerConfiguration()
            //.Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();

        Directory.SetCurrentDirectory(@"C:\work\nadia-build");

        try
        {
            //await DownloadAssets(assets);
            //await ExtarctInstallWim(
            //    "downloads\\windows11_ltsc_26100.1.iso",
            //    "build\\windows11_ltsc_26100.1.wim"
            //);
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo);

            // 1. Windows 11 Enterprise LTSC
            // 2. Windows 11 IoT Enterprise LTSC
            // 3. Windows 11 IoT Enterprise Subscription LTSC
            //MountWim(@"build\windows11_ltsc_26100.1.wim", @"build\windows", 2);

            UnmountWim(@"build\windows", false);
        }
        finally
        {
            DismApi.Shutdown();
        }

        Console.Write("Press enter to continue...");
        Console.ReadLine();
    }

    private static void UnmountWim(string dest, bool commit)
    {
        using (var bar = new DismProgressBar("Unmounting old image..."))
        {
            DismApi.UnmountImage(dest, false, bar.Callback);
        }
    }

    private static void MountWim(string wim, string dest, int index)
    {
        var images = DismApi.GetMountedImages();
        var wimFull = Path.GetFullPath(wim);
        var destFull = Path.GetFullPath(dest);
        foreach (var imageInfo in images)
        {
            if (imageInfo.MountPath == destFull)
            {
                if (imageInfo.ImageFilePath == wimFull && imageInfo.ImageIndex == index)
                {
                    Log.Information($"wim is already mounted, skipping");
                    return;
                }
                else
                {
                    Log.Information(
                        $"path {dest} has been mounted with another image, unmonting it"
                    );
                    Log.Debug(
                        $"expected: {wimFull} {index}; got {imageInfo.ImageFilePath} {imageInfo.ImageIndex}"
                    );
                    UnmountWim(wimFull, false);
                }
            }
        }

        Directory.CreateDirectory(dest);
        Log.Information($"mouting wim {wim} to {dest}...");
        using (var bar = new DismProgressBar("Mouting image..."))
        {
            DismApi.MountImage(wim, dest, index, readOnly: false, progressCallback: bar.Callback);
        }
    }

    private static async Task ExtarctInstallWim(string iso, string dest)
    {
        if (File.Exists(dest))
        {
            Log.Information($"{dest} is already exists, skipping extract.");
            return;
        }

        Log.Information($"Extacting install.wim from {iso} to {dest} ...");
        using var file = File.OpenRead(iso);
        using var cd = new UdfReader(file);
        using var installWimStream = cd.OpenFile("sources\\install.wim", FileMode.Open);
        using (var destStream = File.Open($"{dest}.tmp", FileMode.Create))
        {
            destStream.Seek(0, SeekOrigin.Begin);
            await installWimStream.CopyToAsync(destStream);
        }
        File.Move($"{dest}.tmp", dest);
    }

    private static async Task DownloadAssets(AssetMeta[] assets)
    {
        Log.Information("starting aria2 for download...");
        Aria2Downloader.Instance.EnsureProcess();

        Thread.Sleep(200);
        using var downloadProgress = new ProgressBar(
            assets.Length,
            "Downloading Files",
            new ProgressBarOptions
            {
                ProgressCharacter = '-',
                BackgroundCharacter = '.',
                ForegroundColor = ConsoleColor.Green,
                BackgroundColor = ConsoleColor.Gray,
                ProgressBarOnBottom = true,
            }
        );

        var gids = new List<DownloadRecord>();
        foreach (var item in assets)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "downloads", item.FileName);
            var aria2File = $"{file}.aria2";
            if (File.Exists(file) && !File.Exists(aria2File))
            {
                Log.Information($"{item.FileName} already exists");
                continue;
            }

            var gid = await Aria2Downloader.Instance.AddUrl(
                item.Url,
                "downloads",
                item.FileName,
                item.Hash,
                10
            );

            var bar = downloadProgress.Spawn(
                1000,
                item.FileName,
                new ProgressBarOptions
                {
                    ProgressCharacter = '-',
                    BackgroundCharacter = '.',
                    ForegroundColor = ConsoleColor.Green,
                    BackgroundColor = ConsoleColor.Gray,
                    ProgressBarOnBottom = true,
                }
            );
            gids.Add(
                new DownloadRecord
                {
                    FileName = item.FileName,
                    Gid = gid,
                    Bar = bar,
                }
            );

            Log.Information($"added task {gid} for file {item.FileName}");
        }

        while (gids.Count > 0)
        {
            for (var i = gids.Count - 1; i >= 0; i--)
            {
                var g = gids[i];
                var status = await Aria2Downloader.Instance.GetStatus(g.Gid);
                var progress = g.Bar.AsProgress<float>();
                if (status.totalLength > 0 && status.completedLength > 0)
                {
                    if (status.verifiedLength > 0)
                    {
                        progress.Report((float)status.verifiedLength / status.totalLength);
                    }
                    else
                    {
                        progress.Report((float)status.completedLength / status.totalLength);
                    }
                }

                if (status.status == "complete")
                {
                    g.Bar.Message = $"{g.FileName} completed.";
                    progress.Report(1.0f);
                    downloadProgress.Tick();
                    gids.RemoveAt(i);
                }
                else if (status.status == "error")
                {
                    progress.Report(0f);
                    downloadProgress.WriteErrorLine(status.errorMessage);
                    g.Bar.Message = $"{g.FileName} failed.";
                    Log.Error($"failed to download {g.FileName}: ${status.errorMessage}");
                }
                else if (status.status == "waiting")
                {
                    g.Bar.Message = $"{g.FileName} waiting...";
                }
                else if (status.status == "active")
                {
                    if (
                        status.totalLength == status.completedLength
                        && status.verifiedLength < status.totalLength
                    )
                    {
                        if (status.verifyIntegrityPending)
                        {
                            g.Bar.Message =
                                $"{g.FileName} download completed, waiting for verify...";
                        }
                        else
                        {
                            g.Bar.Message = $"{g.FileName} verifing...";
                        }
                    }
                    else
                    {
                        g.Bar.Message =
                            $"{g.FileName} {(status.downloadSpeed / 1024f / 1024f):F2}M/s";
                    }
                }
            }
            Thread.Sleep(500);
        }
    }
}
