using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace nadia.Presets;

public class EnglishToChinese : IAsyncPreset
{
    public required string MountDir;
    public required string LanguagePackDir;
    public required string LofIso;

    public async Task RunAsync()
    {
        Directory.CreateDirectory(LanguagePackDir);

        var languagePacks = new[]
        {
            "Microsoft-Windows-Client-Language-Pack_x64_zh-cn.cab",
            "Microsoft-Windows-LanguageFeatures-TextToSpeech-zh-cn-Package~31bf3856ad364e35~amd64~~.cab",
            "Microsoft-Windows-LanguageFeatures-Speech-zh-cn-Package~31bf3856ad364e35~amd64~~.cab",
            "Microsoft-Windows-LanguageFeatures-Fonts-Hans-Package~31bf3856ad364e35~amd64~~.cab",
            "Microsoft-Windows-LanguageFeatures-Basic-zh-cn-Package~31bf3856ad364e35~amd64~~.cab",
            "Microsoft-Windows-LanguageFeatures-OCR-zh-cn-Package~31bf3856ad364e35~amd64~~.cab",
            "Microsoft-Windows-LanguageFeatures-Handwriting-zh-cn-Package~31bf3856ad364e35~amd64~~.cab",
        };

        await IsoUtils.ExtractLanguagePacks(languagePacks, LofIso, LanguagePackDir);

        try
        {
            DismUtils.AddPackages(
                MountDir,
                languagePacks.Select((i) => Path.Join(LanguagePackDir, i)).ToArray()
            );
            await DismUtils.SetDefaultLanguage(MountDir, "zh-CN");
            DismUtils.RemoveLanguagePackages(MountDir, "en-US");
        }
        catch (Exception ex)
        {
            Log.Warning("failed to finish language setup", ex);
        }
    }
}
