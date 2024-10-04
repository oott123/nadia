using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace nadia.Runners;

public class Tiny11Core : BaseRunner
{
    public override async Task Run(JObject? args)
    {
        // Bypassing system requirements(on the system image)
        RegistryUtils.RegSetValue(
            Registry.UserDefault,
            @"Control Panel\UnsupportedHardwareNotificationCache",
            "SV1",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.UserDefault,
            @"Control Panel\UnsupportedHardwareNotificationCache",
            "SV2",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Control Panel\UnsupportedHardwareNotificationCache",
            "SV1",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Control Panel\UnsupportedHardwareNotificationCache",
            "SV2",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"Setup\LabConfig",
            "BypassCPUCheck",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"Setup\LabConfig",
            "BypassRAMCheck",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"Setup\LabConfig",
            "BypassSecureBootCheck",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"Setup\LabConfig",
            "BypassStorageCheck",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"Setup\LabConfig",
            "BypassTPMCheck",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"Setup\MoSetup",
            "AllowUpgradesWithUnsupportedTPMOrCPU",
            "1"
        );

        // Disabling Sponsored Apps

        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "OemPreInstalledAppsEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "PreInstalledAppsEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SilentInstalledAppsEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\CloudContent",
            "DisableWindowsConsumerFeatures",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "ContentDeliveryAllowed",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Microsoft\PolicyManager\current\device\Start",
            "ConfigureStartPins",
            "{\"pinnedList\": [{}]}",
            RegistryValueKind.String
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "ContentDeliveryAllowed",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "ContentDeliveryAllowed",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "FeatureManagementEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "OemPreInstalledAppsEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "PreInstalledAppsEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "PreInstalledAppsEverEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SilentInstalledAppsEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SoftLandingEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SubscribedContentEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SubscribedContent-310093Enabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SubscribedContent-338388Enabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SubscribedContent-338389Enabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SubscribedContent-338393Enabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SubscribedContent-353694Enabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SubscribedContent-353696Enabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SubscribedContentEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
            "SystemPaneSuggestionsEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\PushToInstall",
            "DisablePushToInstall",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\MRT",
            "DontOfferThroughWUAU",
            "1"
        );
        RegistryUtils.RegDelete(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\Subscriptions"
        );
        RegistryUtils.RegDelete(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SuggestedApps"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\CloudContent",
            "DisableConsumerAccountStateContent",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\CloudContent",
            "DisableCloudOptimizedContent",
            "1"
        );

        // OOBE Adjust
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Microsoft\Windows\CurrentVersion\OOBE",
            "BypassNRO",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Microsoft\Windows\CurrentVersion\OOBE",
            "DisableOnline",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Microsoft\Windows\CurrentVersion\OOBE",
            "PrivacyConsentStatus",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Microsoft\Windows\CurrentVersion\OOBE",
            "DisablePrivacyExperience",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Microsoft\Windows\CurrentVersion\OOBE",
            "ProtectYourPC",
            "3"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\CloudExperienceHost\Intent\PersonalDataExport",
            "PDEShown",
            "2"
        );

        // Disabling Reserved Storage
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Microsoft\Windows\CurrentVersion\ReserveManager",
            "ShippedWithReserves",
            "0"
        );

        // Disabling Chat icon
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\Windows Chat",
            "ChatIcon",
            "3"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "TaskbarMn",
            "0"
        );

        // Disabling Telemetry
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
            "Enabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Windows\CurrentVersion\Privacy",
            "TailoredExperiencesWithDiagnosticDataEnabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy",
            "HasAccepted",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Input\TIPC",
            "Enabled",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\InputPersonalization",
            "RestrictImplicitInkCollection",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\InputPersonalization",
            "RestrictImplicitTextCollection",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\InputPersonalization\TrainedDataStore",
            "HarvestContacts",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Microsoft\Personalization\Settings",
            "AcceptedPrivacyPolicy",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\DataCollection",
            "AllowTelemetry",
            "0"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSystem,
            @"ControlSet001\Services\dmwappushservice",
            "Start",
            "4"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\Windows Chat",
            "ChatIcon",
            "3"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
            "TaskbarMn",
            "0"
        );

        // Disabling OneDrive folder backup
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\OneDrive",
            "DisableFileSyncNGSC",
            "1"
        );

        // Disabling bing in Start Menu
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Policies\Microsoft\Windows\Explorer",
            "ShowRunAsDifferentUserInStart",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.NtUser,
            @"Software\Policies\Microsoft\Windows\Explorer",
            "DisableSearchBoxSuggestions",
            "1"
        );

        // cleanup task
        RegistryUtils.MakeRegistryWriable(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks"
        );

        // 'Deleting Application Compatibility Appraiser'
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{0600DD45-FAF2-4131-A006-0B17509B9F78}"
        );

        // 'Deleting Customer Experience Improvement Program'
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{4738DE7A-BCC1-4E2D-B1B0-CADB044BFA81}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{6FAC31FA-4A85-4E64-BFD5-2154FF4594B3}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{FC931F16-B50A-472E-B061-B6F79A71EF59}"
        );

        // 'Deleting Program Data Updater'
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{0671EB05-7D95-4153-A32B-1426B9FE61DB}"
        );

        // 'Deleting autochk proxy'
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{87BF85F4-2CE1-4160-96EA-52F554AA28A2}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{8A9C643C-3D74-4099-B6BD-9C6D170898B1}"
        );

        // 'Deleting QueueReporting'
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{E3176A65-4E44-4ED3-AA73-3283660ACB9C}"
        );

        // Disable Windows Update
        RegistryUtils.DisableService(Registry, "wuauserv");
        RegistryUtils.DisableService(Registry, "UsoSvc");
        RegistryUtils.DisableService(Registry, "uhssvc");
        RegistryUtils.DisableService(Registry, "WaaSMedicSvc");
        FileUtils.TakeOwnAndDelete(Path.Join(MountDir, "Windows", "System32", "WaaSMedicSvc.dll"));
        FileUtils.TakeOwnAndDelete(Path.Join(MountDir, "Windows", "System32", "wuaueng.dll"));
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\WindowsUpdate",
            "DoNotConnectToWindowsUpdateInternetLocations",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\WindowsUpdate",
            "DisableWindowsUpdateAccess",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\WindowsUpdate",
            "WUServer",
            "localhost",
            RegistryValueKind.String
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\WindowsUpdate",
            "WUStatusServer",
            "localhost",
            RegistryValueKind.String
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\WindowsUpdate",
            "UpdateServiceUrlAlternate",
            "localhost",
            RegistryValueKind.String
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\WindowsUpdate\AU",
            "UseWUServer",
            "1"
        );
        RegistryUtils.RegSetValue(
            Registry.MachineSoftware,
            @"Policies\Microsoft\Windows\WindowsUpdate\AU",
            "NoAutoUpdate",
            "1"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{2540477E-E654-4302-AD44-383BBFFBFF16}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{341B2255-6A6B-442A-AF5A-C610B7DBE12D}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{476E8CFA-78E2-4C51-854E-538F8643B4FD}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{764DDB74-CB08-4E0A-8580-B41F94F2C7BE}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{817CCFDD-4DD0-4102-AC6E-3F5D3B789FB8}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{99CEDA8C-A866-4787-BBD3-6F3C9F61DD5C}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{9B3CDCDA-4197-490B-AA5C-C9F5F42A9D88}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{9CBBFAAE-DB9F-48B4-BAC0-4CFF482A4E01}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{A31197EC-EAEE-4837-8A9C-3A17D358B9EB}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{B4FBEFA9-6F7C-4C74-A891-3774B7BCD072}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{B53BD60A-5823-411C-9C75-AA91DB3C35F8}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{CECDC345-7460-4A15-9D8B-DAC3F9CC5368}"
        );
        RegistryUtils.RegDelete(
            Registry.MachineSoftware,
            @"Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tasks\{E0F10DCF-44AD-40E8-9370-FB5DA59F93FB}"
        );
        RegistryUtils.RegDelete(Registry.MachineSystem, @"ControlSet001\Services\WaaSMedicSVC");
        RegistryUtils.RegDelete(Registry.MachineSystem, @"ControlSet001\Services\UsoSvc");
    }
}
