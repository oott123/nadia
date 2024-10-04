using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;

namespace nadia.Runners;

public record class EnglishToChineseArgs
{
    public required string LofIso { get; init; }
}

public class EnglishToChinese : BaseRunner
{
    public required string LanguagePackDir = @"build\language_packs";

    public override async Task Run(JObject? args)
    {
        var ap = args.ToObject<EnglishToChineseArgs>();

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

        await IsoUtils.ExtractLanguagePacks(languagePacks, ap.LofIso, LanguagePackDir);

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
