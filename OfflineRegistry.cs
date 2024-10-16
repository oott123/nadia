using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace nadia
{
    public record class OfflineRegistry
    {
        public required Hive MachineSystem { get; init; }
        public required Hive MachineSoftware { get; init; }
        public required Hive UserDefault { get; init; }
        public required Hive NtUser { get; init; }

        private bool _loaded = true;

        public static OfflineRegistry MountRegistry(string mount)
        {
            Log.Information("loading registry");
            return new OfflineRegistry()
            {
                MachineSystem = Hive.LoadFromFile(
                    Path.Join(mount, "Windows", "System32", "config", "SYSTEM"),
                    "OfflineSystem"
                ),
                MachineSoftware = Hive.LoadFromFile(
                    Path.Join(mount, "Windows", "System32", "config", "SOFTWARE"),
                    "OfflineSoftware"
                ),
                UserDefault = Hive.LoadFromFile(
                    Path.Join(mount, "Windows", "System32", "config", "DEFAULT"),
                    "OfflineUserDefault"
                ),
                NtUser = Hive.LoadFromFile(
                    Path.Join(mount, "Users", "Default", "ntuser.dat"),
                    "OfflineNtUser"
                ),
                _loaded = true,
            };
        }

        public void SaveRegistry()
        {
            if (_loaded)
            {
                Log.Information("saving registry");
                UserDefault.SaveAndUnload();
                MachineSoftware.SaveAndUnload();
                MachineSystem.SaveAndUnload();
                NtUser.SaveAndUnload();
                _loaded = false;
            }
        }
    }
}
