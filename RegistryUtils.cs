using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Serilog;

namespace nadia;

public static class RegistryUtils
{
    public static void MakeRegistryWriable(Hive hive, string subPath)
    {
        var subKeys = new Queue<string>();
        subKeys.Enqueue(subPath);

        var identity = WindowsIdentity.GetCurrent().User;

        while (subKeys.Count > 0)
        {
            var sp = subKeys.Dequeue();

            using var subKey = hive.RootKey.OpenSubKey(
                sp,
                RegistryKeyPermissionCheck.ReadWriteSubTree,
                RegistryRights.TakeOwnership
            );

            if (subKey == null)
            {
                continue;
            }

            var sec = new RegistrySecurity();

            sec.SetOwner(identity);
            subKey.SetAccessControl(sec);

            sec.AddAccessRule(
                new RegistryAccessRule(
                    identity,
                    RegistryRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow
                )
            );
            subKey.SetAccessControl(sec);
            subKey.Close();

            using var subKey2 = hive.RootKey.OpenSubKey(sp, false);

            foreach (var key in subKey2.GetSubKeyNames())
            {
                subKeys.Enqueue(Path.Join(sp, key));
            }

            subKey.Close();
        }
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
        using var subKey = rootKey.OpenSubKey(path, true) ?? rootKey.CreateSubKey(path, true);

        if (kind == RegistryValueKind.DWord)
        {
            subKey.SetValue(name, unchecked((int)uint.Parse(value)), kind);
        }
        else
        {
            subKey.SetValue(name, value, kind);
        }
        subKey.Close();
    }

    public static void DisableService(OfflineRegistry registry, string service)
    {
        SetServiceStart(registry, service, 4);
    }

    public static void SetServiceStart(OfflineRegistry registry, string service, int start)
    {
        Log.Information($"setting service start: {service} = {start}");
        try
        {
            using var def = registry.MachineSystem.RootKey.OpenSubKey(
                $"ControlSet001\\Services\\{service}",
                true
            );
            if (def == null)
            {
                Log.Warning($"service {service} is not exists");
            }
            else
            {
                def.SetValue("Start", start, RegistryValueKind.DWord);
            }
        }
        catch (SecurityException)
        {
            MakeRegistryWriable(registry.MachineSystem, $"ControlSet001\\Services\\{service}");
            using var def = registry.MachineSystem.RootKey.OpenSubKey(
                $"ControlSet001\\Services\\{service}",
                true
            );
            def?.SetValue("Start", start, RegistryValueKind.DWord);
        }
    }

    public static void RegDelete(Hive hive, string key)
    {
        using var parentKey = hive.RootKey.OpenSubKey(key, false);
        if (parentKey == null)
        {
            Log.Debug($"registry key {key} is not exists");
            return;
        }
        MakeRegistryWriable(hive, key);
        hive.RootKey.DeleteSubKeyTree(key);
    }
}
