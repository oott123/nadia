using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;
using static nadia.WinUtilTweakService;

namespace nadia.Runners;

public record class CopyFileArgs
{
    public required string Src { get; init; }
    public required string Dst { get; init; }
}

public class CopyFile : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<CopyFileArgs>();
        var findDirs = new string[]
        {
            Path.Join(Directory.GetCurrentDirectory()),
            Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
        };

        foreach (var dir in findDirs)
        {
            var file = Path.Join(dir, ap.Src);
            if (File.Exists(file))
            {
                var dest = Path.Join(MountDir, Path.GetDirectoryName(ap.Dst));
                Directory.CreateDirectory(dest);
                Log.Information($"copying {file} to {ap.Dst}");

                File.Copy(file, Path.Join(MountDir, ap.Dst), true);
                break;
            }
            else if (Directory.Exists(file))
            {
                var dest = Path.Join(MountDir, ap.Dst);
                Directory.CreateDirectory(dest);
                Log.Information($"copying directory {file} to {ap.Dst}");
                DirectoryCopy(file, dest);
            }
        }
    }

    private static void DirectoryCopy(
        string sourceBasePath,
        string destinationBasePath,
        bool recursive = true
    )
    {
        if (!Directory.Exists(sourceBasePath))
            throw new DirectoryNotFoundException($"Directory '{sourceBasePath}' not found");

        var directoriesToProcess = new Queue<(string sourcePath, string destinationPath)>();
        directoriesToProcess.Enqueue(
            (sourcePath: sourceBasePath, destinationPath: destinationBasePath)
        );
        while (directoriesToProcess.Any())
        {
            (string sourcePath, string destinationPath) = directoriesToProcess.Dequeue();

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            var sourceDirectoryInfo = new DirectoryInfo(sourcePath);
            foreach (FileInfo sourceFileInfo in sourceDirectoryInfo.EnumerateFiles())
                sourceFileInfo.CopyTo(Path.Combine(destinationPath, sourceFileInfo.Name), true);

            if (!recursive)
                continue;

            foreach (
                DirectoryInfo sourceSubDirectoryInfo in sourceDirectoryInfo.EnumerateDirectories()
            )
                directoriesToProcess.Enqueue(
                    (
                        sourcePath: sourceSubDirectoryInfo.FullName,
                        destinationPath: Path.Combine(destinationPath, sourceSubDirectoryInfo.Name)
                    )
                );
        }
    }
}
