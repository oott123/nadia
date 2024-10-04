using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nadia;
using Serilog;
using ShellProgressBar;

namespace nadia
{
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

    public static class DownloadUtils
    {
        public static async Task DownloadAssets(AssetMeta[] assets)
        {
            Log.Information("starting aria2 for download...");
            Aria2Downloader.Instance.EnsureProcess();

            Thread.Sleep(200);
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
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
                var file = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "downloads",
                    item.FileName
                );
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
}
