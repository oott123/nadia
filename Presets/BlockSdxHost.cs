using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nadia.Presets;

public class BlockSdxHost : IPreset
{
    public required string MountDir;

    public void Run()
    {
        var hostsFile = Path.Join(MountDir, "Windows", "System32", "drivers", "etc", "hosts");
        var hosts = File.ReadAllText(hostsFile, Encoding.ASCII);
        hosts += "\r\n0.0.0.0 sdx.microsoft.com";
        File.WriteAllText(hostsFile, hosts, Encoding.ASCII);
    }
}
