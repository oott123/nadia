using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Serilog;

namespace nadia
{
    public class Hive
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegLoadKey(IntPtr hKey, string lpSubKey, string lpFile);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegSaveKey(IntPtr hKey, string lpFile, uint securityAttrPtr = 0);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegUnLoadKey(IntPtr hKey, string lpSubKey);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern IntPtr RtlAdjustPrivilege(
            int Privilege,
            bool bEnablePrivilege,
            bool IsThreadPrivilege,
            out bool PreviousValue
        );

        [DllImport("advapi32.dll")]
        static extern bool LookupPrivilegeValue(
            string lpSystemName,
            string lpName,
            ref UInt64 lpLuid
        );

        [DllImport("advapi32.dll")]
        static extern bool LookupPrivilegeValue(
            IntPtr lpSystemName,
            string lpName,
            ref UInt64 lpLuid
        );

        private RegistryKey parentKey;
        private string name;
        private string originalPath;
        public RegistryKey RootKey;

        private Hive() { }

        public static Hive LoadFromFile(string Path, string subName)
        {
            AcquirePrivileges();
            Hive result = new Hive();

            result.parentKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine,
                RegistryView.Default
            );
            result.name = subName;
            result.originalPath = Path;
            IntPtr parentHandle = result.parentKey.Handle.DangerousGetHandle();
            var err = RegLoadKey(parentHandle, result.name, Path);
            if (err != 0)
            {
                throw new System.ComponentModel.Win32Exception(err);
            }
            result.RootKey = result.parentKey.OpenSubKey(result.name, true);
            return result;
        }

        public static void AcquirePrivileges()
        {
            ulong luid = 0;
            bool throwaway;
            LookupPrivilegeValue(IntPtr.Zero, "SeRestorePrivilege", ref luid);
            RtlAdjustPrivilege((int)luid, true, false, out throwaway);
            LookupPrivilegeValue(IntPtr.Zero, "SeBackupPrivilege", ref luid);
            RtlAdjustPrivilege((int)luid, true, false, out throwaway);
        }

        public static void ReturnPrivileges()
        {
            ulong luid = 0;
            bool throwaway;
            LookupPrivilegeValue(IntPtr.Zero, "SeRestorePrivilege", ref luid);
            RtlAdjustPrivilege((int)luid, false, false, out throwaway);
            LookupPrivilegeValue(IntPtr.Zero, "SeBackupPrivilege", ref luid);
            RtlAdjustPrivilege((int)luid, false, false, out throwaway);
        }

        public void SaveAndUnload()
        {
            RootKey.Flush();
            RootKey.Close();
            parentKey.Flush();

            var err = 0;
            for (var i = 0; i < 10; i++)
            {
                err = RegUnLoadKey(parentKey.Handle.DangerousGetHandle(), name);

                if (err == 5)
                {
                    Log.Warning("cannot save registry key, wait for 5 seconds");
                    Thread.Sleep(5000);
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (err != 0)
            {
                throw new System.ComponentModel.Win32Exception(err);
            }

            parentKey.Close();
        }

        public string Name
        {
            get { return name; }
        }
    }
}
