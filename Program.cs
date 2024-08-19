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
                FileName = "windows11_ltsc.iso",
                Hash = "aaa4bd3254c1af5f9ce07f50db68fdead7a305878f2425c059ecd6b062a855b3",
            },
            new()
            {
                Url =
                [
                    "https://drive.massgrave.dev/26100.1.240331-1435.ge_release_amd64fre_CLIENT_LOF_PACKAGES_OEM.iso",
                ],
                FileName = "windows11_ltsc_client_lof.iso",
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
        };

        Log.Logger = new LoggerConfiguration()
            //.Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();

        await DownloadAssets(assets);
    }

    private static async Task DownloadAssets(AssetMeta[] assets)
    {
        Log.Information("starting aria2 for download...");
        Aria2Downloader.Instance.EnsureProcess();

        Thread.Sleep(500);
        using var downloadProgress = new ProgressBar(
            assets.Length,
            "Downloading Files",
            new ProgressBarOptions
            {
                ProgressCharacter = '-',
                BackgroundCharacter = '.',
                ForegroundColor = ConsoleColor.Green,
                BackgroundColor = ConsoleColor.Gray,
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
