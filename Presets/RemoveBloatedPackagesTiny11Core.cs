using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dism;
using Serilog;

namespace nadia.Presets;

public class RemoveBloatedPackagesTiny11Core : IPreset
{
    public required string MountDir;

    public void Run()
    {
        using var session = DismApi.OpenOfflineSession(MountDir);
        var packages = DismApi.GetPackages(session);

        var packagesToRemove = new[]
        {
            "Clipchamp.Clipchamp_",
            "Microsoft.BingNews_",
            "Microsoft.BingWeather_",
            "Microsoft.GamingApp_",
            "Microsoft.GetHelp_",
            "Microsoft.Getstarted_",
            "Microsoft.MicrosoftOfficeHub_",
            "Microsoft.MicrosoftSolitaireCollection_",
            "Microsoft.People_",
            "Microsoft.PowerAutomateDesktop_",
            "Microsoft.Todos_",
            "Microsoft.WindowsAlarms_",
            "microsoft.windowscommunicationsapps_",
            "Microsoft.WindowsFeedbackHub_",
            "Microsoft.WindowsMaps_",
            "Microsoft.WindowsSoundRecorder_",
            "Microsoft.Xbox.TCUI_",
            "Microsoft.XboxGamingOverlay_",
            "Microsoft.XboxGameOverlay_",
            "Microsoft.XboxSpeechToTextOverlay_",
            "Microsoft.YourPhone_",
            "Microsoft.ZuneMusic_",
            "Microsoft.ZuneVideo_",
            "MicrosoftCorporationII.MicrosoftFamily_",
            "MicrosoftCorporationII.QuickAssist_",
            "MicrosoftTeams_",
            "Microsoft.549981C3F5F10_",
        };
        foreach (var package in packages)
        {
            foreach (var packageName in packagesToRemove)
            {
                if (package.PackageName.StartsWith(packageName))
                {
                    Log.Information($"removing package {package.PackageName}...");
                    try
                    {
                        DismApi.RemovePackageByName(session, package.PackageName);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"failed to remove package {package.PackageName} {ex.Message}");
                    }
                    break;
                }
            }
        }
    }
}
