using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using Autofac;
using Autofac.Features.ResolveAnything;
using DiscUtils.Iso9660;
using DiscUtils.Udf;
using lzo.net;
using Microsoft.Dism;
using Microsoft.Win32;
using nadia;
using nadia.Runners;
using nadia.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProcessPrivileges;
using Serilog;
using ShellProgressBar;
using static nadia.WinUtilTweakService;

namespace Nadia;

public record class TaskStep
{
    public required string Task { get; init; }
    public required bool? Skip { get; init; }
    public required JObject Args { get; init; }
}

public record class TaskDefinition
{
    public required string WorkDir { get; init; }
    public required TaskStep[] Steps { get; init; }
}

class Program
{
    static async Task Main(string[] args)
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterInstance(new MountProvider());
        containerBuilder.RegisterInstance(new OfflineRegistryProvider());
        containerBuilder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
        var container = containerBuilder.Build();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();

        var taskFile = "";
        var findDirs = new string[]
        {
            Path.Join(Assembly.GetExecutingAssembly().Location, "..", "tasks"),
            Path.Join(Directory.GetCurrentDirectory(), "tasks"),
        };

        foreach (var dir in findDirs)
        {
            var file = Path.Join(dir, "win11_iot_ltsc_zh-cn_26100.1742.json");
            if (File.Exists(file))
            {
                taskFile = file;
                break;
            }
        }

        if (string.IsNullOrEmpty(taskFile))
        {
            Log.Error($"task file not found");
#if DEBUG
            Debugger.Break();
#endif
            Environment.Exit(1);
            return;
        }

        var task = JsonConvert.DeserializeObject<TaskDefinition>(File.ReadAllText(taskFile));
        var steps = task.Steps.Where(s => s.Skip == null || s.Skip == false);

        var runners = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.Namespace == "nadia.Runners"
                && t.IsClass
                && t.IsPublic
                && !t.IsNested
                && !t.IsAbstract
            )
            .ToDictionary(t => t.Name);

        var ok = true;
        foreach (var step in steps)
        {
            if (!runners.ContainsKey(step.Task))
            {
                Log.Error($"no such task: {step.Task}");
                ok = false;
            }
        }
        if (!ok)
        {
#if DEBUG
            Debugger.Break();
#endif
            Environment.Exit(2);
            return;
        }

        Directory.SetCurrentDirectory(task.WorkDir);
        using var privilegeEnabler = new PrivilegeEnabler(
            Process.GetCurrentProcess(),
            [Privilege.TakeOwnership, Privilege.Restore, Privilege.Backup]
        );

        var disposableRunners = new List<IDisposable>();
        try
        {
            DismApi.Initialize(DismLogLevel.LogErrorsWarningsInfo);

            foreach (var step in steps)
            {
                Log.Information($"processing {step.Task}...");
                var runnerInstance = container.Resolve(runners[step.Task]) as BaseRunner;
                if (runnerInstance == null)
                {
                    Log.Error($"cannot resolve task runner for task {step.Task}");
                    continue;
                }

                await runnerInstance.Run(step.Args);

                if (runnerInstance is IDisposable disposable)
                {
                    disposableRunners.Add(disposable);
                }
            }
        }
        finally
        {
            Log.Information("--- cleaning up ---");
            foreach (var d in disposableRunners.Reverse<IDisposable>())
            {
                d.Dispose();
            }
            DismApi.Shutdown();
        }
    }
}
