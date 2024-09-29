using System.Collections.Generic;
using System.Diagnostics;
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
            using var privilegeEnabler = new PrivilegeEnabler(
                Process.GetCurrentProcess(),
                [Privilege.TakeOwnership, Privilege.Restore, Privilege.Backup]
            );

            await DownloadUtils.DownloadAssets(assets);
            await IsoUtils.ExtractFromIso(
                "downloads\\windows11_ltsc_26100.1.iso",
                "sources\\install.wim",
                "build\\windows11_ltsc_26100.1.wim"
            );
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo);

            var mountDir = @"build\windows";
            var wim = @"build\windows11_ltsc_26100.1.wim";
            var outWim = @"build\nadia_win11_ltsc_26100.wim";

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

            new RemoveBloatedPackagesTiny11Core { MountDir = mountDir }.Run();

            var registry = OfflineRegistry.MountRegistry(mountDir);

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

            registry.SaveRegistry();

            new CleanupSxsTiny11Core { MountDir = mountDir }.Run();

            await DismUtils.CleanupImage(mountDir);
            DismUtils.UnmountWim(mountDir, true);
            await DismUtils.RebuildImage(wim, outWim, wimIndex);
        }
        finally
        {
            DismApi.Shutdown();
        }

        Console.Write("Press enter to continue...");
        Console.ReadLine();
    }
}
