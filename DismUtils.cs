using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dism;
using Serilog;

namespace nadia
{
    public static class DismUtils
    {
        public static async Task RebuildImage(string source, string dest, int index)
        {
            var srcPath = Path.GetFullPath(source);
            var dstPath = Path.GetFullPath(dest);
            if (File.Exists(dstPath))
            {
                File.Delete(dstPath);
            }

            Log.Information($"call dism.exe to rebuild image {index}...");
            await Process
                .Start(
                    new ProcessStartInfo()
                    {
                        FileName = "dism.exe",
                        Arguments =
                            $"/Export-Image /SourceImageFile:\"{srcPath}\" /SourceIndex:{index} /DestinationImageFile:\"{dstPath}\" /Compress:max /checkintegrity",
                    }
                )!
                .WaitForExitAsync();
        }

        public static async Task CleanupImage(string mountPath)
        {
            var mount = Path.GetFullPath(mountPath);

            Log.Information($"call dism.exe to clean up image...");
            await Process
                .Start(
                    new ProcessStartInfo()
                    {
                        FileName = "dism.exe",
                        Arguments =
                            $"/Image:\"{mount}\" /Cleanup-Image /StartComponentCleanup /ResetBase",
                    }
                )!
                .WaitForExitAsync();
        }

        public static async Task DeleteImage(string wim, string index)
        {
            var wimPath = Path.GetFullPath(wim);

            Log.Information($"call dism.exe to delete image index {index}...");
            await Process
                .Start(
                    new ProcessStartInfo()
                    {
                        FileName = "dism.exe",
                        Arguments = $"/Delete-Image /ImageFile:\"{wimPath}\" /Index:{index}",
                    }
                )!
                .WaitForExitAsync();
        }

        public static async Task SetDefaultLanguage(string mountPath, string lang)
        {
            var mount = Path.GetFullPath(mountPath);

            Log.Information($"call dism.exe to set all intl to {lang}...");
            await Process
                .Start(
                    new ProcessStartInfo()
                    {
                        FileName = "dism.exe",
                        Arguments = $"/Image:\"{mount}\" /Set-AllIntl:{lang}",
                    }
                )!
                .WaitForExitAsync();
        }

        public static void RemoveLanguagePackages(string mount, string language)
        {
            using (var session = DismApi.OpenOfflineSession(mount))
            {
                var packages = DismApi.GetPackages(session);
                Log.Information($"find and delete {language} language packages...");
                foreach (var package in packages)
                {
                    if (
                        package.PackageName.ToLower().Contains(language.ToLower())
                        && package.PackageState == DismPackageFeatureState.Installed
                        && package.ReleaseType == DismReleaseType.LanguagePack
                        && !package.PackageName.StartsWith(
                            "Microsoft-Windows-LanguageFeatures-Basic"
                        ) // 永久性，不可删除
                    )
                    {
                        Log.Information($"removing package {package.PackageName}...");
                        try
                        {
                            DismApi.RemovePackageByName(session, package.PackageName);
                        }
                        catch (Exception ex)
                        {
                            Log.Information(
                                $"failed to remove package {package.PackageName}: {ex.Message}. that's okay, ignore it."
                            );
                        }
                    }
                }
            }
        }

        public static void AddLanguagePacks(string mount, string src, string[] languagePacks)
        {
            using (var session = DismApi.OpenOfflineSession(mount))
            {
                foreach (var s in languagePacks)
                {
                    Log.Information($"adding {s}");
                    DismApi.AddPackage(session, Path.GetFullPath($"{src}\\{s}"), false, false);
                }
            }
        }

        public static void UnmountWim(string dest, bool commit)
        {
            using (var bar = new DismProgressBar("Unmounting image..."))
            {
                DismApi.UnmountImage(dest, commit, bar.Callback);
            }
        }

        public static void MountWim(string wim, string dest, int index)
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
                DismApi.MountImage(
                    wim,
                    dest,
                    index,
                    readOnly: false,
                    progressCallback: bar.Callback
                );
            }
        }
    }
}
