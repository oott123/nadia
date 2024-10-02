using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using DiscUtils.Iso9660;
using DiscUtils.Udf;
using lzo.net;
using Microsoft.Dism;
using Microsoft.Win32;
using nadia;
using nadia.Presets;
using ProcessPrivileges;
using Serilog;
using ShellProgressBar;
using static nadia.WinUtilTweakService;

namespace Nadia;

class Program
{
    static async Task Main(string[] args)
    {
        var assets = new AssetMeta[]
        {
            new()
            {
                Url =
                [
                    "https://drive.massgrave.dev/X23-81951_26100.1742.240906-0331.ge_release_svc_refresh_CLIENT_ENTERPRISES_OEM_x64FRE_en-us.iso",
                ],
                FileName = "windows11_ltsc_26100.1742.iso",
                Hash = "4f59662a96fc1da48c1b415d6c369d08af55ddd64e8f1c84e0166d9e50405d7a",
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

        Directory.SetCurrentDirectory(@"C:\work\nadia-build");

        try
        {
            var mountDir = @"build\windows";
            var wim = @"build\windows11_ltsc_26100.1.wim";
            var outWim = @"build\nadia_win11_ltsc_26100.wim";

            using var privilegeEnabler = new PrivilegeEnabler(
                Process.GetCurrentProcess(),
                [Privilege.TakeOwnership, Privilege.Restore, Privilege.Backup]
            );

            await DownloadUtils.DownloadAssets(assets);
            await IsoUtils.ExtractFromIso(
                "downloads\\windows11_ltsc_26100.1.iso",
                "sources\\install.wim",
                wim
            );
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo);

            // 1. Windows 11 Enterprise LTSC
            // 2. Windows 11 IoT Enterprise LTSC
            // 3. Windows 11 IoT Enterprise Subscription LTSC
            var wimImage = DismApi
                .GetImageInfo(wim)
                .Where((i) => i.ImageName == "Windows 11 IoT Enterprise LTSC")
                .First();
            var wimIndex = wimImage.ImageIndex;

            DismUtils.MountWim(wim, mountDir, wimIndex);

            await (
                new EnglishToChinese
                {
                    LanguagePackDir = @"build\language_packs",
                    LofIso = @"downloads\windows11_ltsc_client_lof_26100.1.iso",
                    MountDir = mountDir,
                }
            ).RunAsync();

            var updateDir = "updates";
            if (Directory.Exists(updateDir))
            {
                Log.Information("installing updates");
                var updateFiles = Directory.EnumerateFiles(
                    updateDir,
                    "*.msu",
                    new EnumerationOptions { RecurseSubdirectories = true }
                );
                DismUtils.AddPackages(mountDir, updateFiles.ToArray());
            }
            else
            {
                Log.Information("unable to find updates directory, skipping");
            }

            new RemoveBloatedPackagesTiny11Core { MountDir = mountDir }.Run();

            var registry = OfflineRegistry.MountRegistry(mountDir);
            try
            {
                new RemoveWindowsDefender { MountDir = mountDir, Registry = registry }.Run();
                new RemoveEdge { MountDir = mountDir, Registry = registry }.Run();
                new RemoveOneDrive { MountDir = mountDir }.Run();
                new Tiny11Core { MountDir = mountDir, Registry = registry }.Run();
                new SkipFirstLogonAnimation { Registry = registry }.Run();
                new CleanupXboxGameBar { Registry = registry }.Run();
                new DisableSmartScreen { Registry = registry }.Run();
                new DisablePagingFile { Registry = registry }.Run();
                new DisableSwapFile { Registry = registry }.Run();
                new DisableHibernation { Registry = registry }.Run();
                new DisableWindowArrangement { Registry = registry }.Run();
                new DisableVulnerableDriverBlocklist { Registry = registry }.Run();
                new AllowExecutionPowershell { Registry = registry }.Run();
                new RealTimeIsUniversal { Registry = registry }.Run();
                new DetailedBsod { Registry = registry }.Run();
                new DisableStorageSense { Registry = registry }.Run();
                new DisableWindowsSearch { Registry = registry }.Run();

                WinUtil.ApplyTweaks(
                    Path.Join(
                        Assembly.GetExecutingAssembly().Location,
                        "..",
                        "winutil",
                        "tweaks.json"
                    ),
                    new string[]
                    {
                        "WPFTweaksAH",
                        "WPFTweaksConsumerFeatures",
                        "WPFTweaksDVR",
                        "WPFTweaksHiber",
                        "WPFTweaksLoc",
                        "WPFTweaksServices",
                        "WPFTweaksTele",
                        "WPFTweaksWifi",
                        "WPFTweaksDisplay",
                        "WPFTweaksDeleteTempFiles",
                        "WPFTweaksEndTaskOnTaskbar",
                        "WPFTweaksIPv46",
                        "WPFTweaksTeredo",
                        "WPFTweaksDisableBGapps",
                        "WPFTweaksRemoveCopilot",
                    },
                    registry
                );

                WinUtil.ApplyTweaks(
                    Path.Join(
                        Assembly.GetExecutingAssembly().Location,
                        "..",
                        "winutil",
                        "custom.json"
                    ),
                    new string[]
                    {
                        "FixOOBEInput",
                        "ImFeelingSafe",
                        "DumberNotification",
                        "GamingOnly",
                    },
                    registry
                );
            }
            finally
            {
                registry.SaveRegistry();
            }

            new BlockSdxHost { MountDir = mountDir }.Run();
            new CleanupSxsTiny11Core { MountDir = mountDir }.Run();

            await DismUtils.CleanupImage(mountDir);
            DismUtils.UnmountWim(mountDir, true);
            await DismUtils.RebuildImage(wim, outWim, wimIndex);
            File.Delete(wim);
        }
        finally
        {
            DismApi.Shutdown();
        }

        Console.Write("Press enter to continue...");
        Console.ReadLine();
    }
}
