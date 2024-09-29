using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscUtils.Udf;
using Serilog;

namespace nadia
{
    public static class IsoUtils
    {
        public static async Task ExtractFromIso(string iso, string name, string dest)
        {
            if (File.Exists(dest))
            {
                Log.Information($"{dest} is already exists, skipping extract.");
                return;
            }

            Log.Information($"Extacting {name} from {iso} to {dest} ...");
            using var file = File.OpenRead(iso);
            using var cd = new UdfReader(file);
            using var installWimStream = cd.OpenFile(name, FileMode.Open);
            using (var destStream = File.Open($"{dest}.tmp", FileMode.Create))
            {
                destStream.Seek(0, SeekOrigin.Begin);
                await installWimStream.CopyToAsync(destStream);
            }
            File.Move($"{dest}.tmp", dest);
        }

        public static async Task ExtractLanguagePacks(
            string[] languagePacks,
            string iso,
            string dest
        )
        {
            foreach (var s in languagePacks)
            {
                await ExtractFromIso(iso, $"LanguagesAndOptionalFeatures\\{s}", $"{dest}\\{s}");
            }
        }
    }
}
