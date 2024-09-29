using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace nadia.Presets;

public class RemoveOneDrive : IPreset
{
    public required string MountDir;

    public void Run()
    {
        Log.Information("removing onedrive");
        FileUtils.TakeOwnAndDelete(Path.Join(MountDir, "Windows", "System32", "OneDriveSetup.exe"));
    }
}
