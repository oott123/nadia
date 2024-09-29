using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Serilog;

namespace nadia;

public static class RegistryUtils
{
    public static void MakeRegistryWriable(Hive hive, string subKey)
    {
        Security.GrantAdministratorsAccess(
            $"MACHINE\\${hive.Name}\\${subKey}",
            Security.SE_OBJECT_TYPE.SE_REGISTRY_KEY
        );
    }

    public static void RegSetValue(
        Hive hive,
        string path,
        string name,
        string value,
        RegistryValueKind kind = RegistryValueKind.DWord
    )
    {
        RegSetValue(hive.RootKey, path, name, value, kind);
    }

    public static void RegSetValue(
        RegistryKey rootKey,
        string path,
        string name,
        string value,
        RegistryValueKind kind = RegistryValueKind.DWord
    )
    {
        var subKey = rootKey.OpenSubKey(path, true);
        if (subKey == null)
        {
            subKey = rootKey.CreateSubKey(path, true);
        }

        if (kind == RegistryValueKind.DWord)
        {
            subKey.SetValue(name, int.Parse(value), kind);
        }
        else
        {
            subKey.SetValue(name, value, kind);
        }
        subKey.Close();
    }

    public static void DisableService(OfflineRegistry registry, string service)
    {
        Log.Information($"disabling service: {service}");
        using var def = registry.MachineSystem.RootKey.OpenSubKey(
            $"ControlSet001\\Services\\{service}",
            true
        );
        def?.SetValue("Start", 4, RegistryValueKind.DWord);
    }

    public static void RegDelete(Hive hive, string key)
    {
        var parentKey = hive.RootKey.OpenSubKey(key);
        if (parentKey == null)
        {
            Log.Debug($"registry key {key} is not exists");
            return;
        }
        parentKey.Close();
        hive.RootKey.DeleteSubKeyTree(key);
    }
}
