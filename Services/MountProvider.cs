using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nadia.Services;

public class MountProvider
{
    public string MountDir { get; set; } = "";
    public string SourceWim { get; set; } = "";
    public int WimIndex { get; set; } = 0;
}
