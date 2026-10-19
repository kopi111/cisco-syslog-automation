using CiscoSyslogAutomation.Models;
using System.Collections.Concurrent;

namespace CiscoSyslogAutomation.Services;

public class ConfigurationOrchestrator
{
    private readonly SyslogSettings _syslogSettings;
    private readonly int _maxParallelism;

    public ConfigurationOrchestrator(SyslogSettings syslogSettings, int maxParallelism)
    {
        _syslogSettings = syslogSettings;
        _maxParallelism = maxParallelism;
    }

    public async Task<IReadOnlyList<ConfigurationResult>> ConfigureAllAsync(IReadOnlyList<NetworkSwitch> switches)
    {
        var results = new ConcurrentBag<ConfigurationResult>();
        var configurator = new SwitchConfigurator(_syslogSettings);
        var semaphore = new SemaphoreSlim(_maxParallelism);

        var tasks = switches.Select(sw => ConfigureWithThrottleAsync(sw, configurator, semaphore, results));
        await Task.WhenAll(tasks);

        return results.ToList().AsReadOnly();
    }

    private static async Task ConfigureWithThrottleAsync(
        NetworkSwitch sw,
        SwitchConfigurator configurator,
        SemaphoreSlim semaphore,
        ConcurrentBag<ConfigurationResult> results)
    {
        await semaphore.WaitAsync();
        try
        {
            var result = await Task.Run(() => configurator.Configure(sw));
            results.Add(result);
            PrintProgress(result);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static void PrintProgress(ConfigurationResult result)
    {
        var label = result.Success ? "[  OK  ]" : "[ FAIL ]";
        var color = result.Success ? ConsoleColor.Green : ConsoleColor.Red;

        Console.ForegroundColor = color;
        Console.Write($"  {label}");
        Console.ResetColor();
        Console.WriteLine($"  {result.Hostname,-20} {result.IpAddress,-16} {result.ErrorMessage ?? string.Empty}");
    }
}
