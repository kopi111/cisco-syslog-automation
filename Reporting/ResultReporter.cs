using CsvHelper;
using CiscoSyslogAutomation.Models;
using System.Globalization;

namespace CiscoSyslogAutomation.Reporting;

public class ResultReporter
{
    public void PrintSummary(IReadOnlyList<ConfigurationResult> results)
    {
        var succeeded = results.Count(r => r.Success);
        var failed = results.Count(r => !r.Success);

        Console.WriteLine("\n--- Deployment Summary ---");
        Console.WriteLine($"  Total:   {results.Count}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Success: {succeeded}");
        Console.ResetColor();

        if (failed > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Failed:  {failed}");
            Console.ResetColor();
            PrintFailedSwitches(results.Where(r => !r.Success));
        }
    }

    public void SaveReport(IReadOnlyList<ConfigurationResult> results, string outputPath)
    {
        using var writer = new StreamWriter(outputPath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(results);
        Console.WriteLine($"\nReport saved to: {Path.GetFullPath(outputPath)}");
    }

    private static void PrintFailedSwitches(IEnumerable<ConfigurationResult> failedSwitches)
    {
        Console.WriteLine("\nFailed Switches:");
        foreach (var result in failedSwitches)
            Console.WriteLine($"  {result.Hostname} ({result.IpAddress}): {result.ErrorMessage}");
    }
}
