using CsvHelper;
using CsvHelper.Configuration;
using CiscoSyslogAutomation.Models;
using System.Globalization;

namespace CiscoSyslogAutomation.Services;

public class InventoryReader
{
    private readonly string _filePath;

    public InventoryReader(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Inventory file not found: {filePath}");

        _filePath = filePath;
    }

    public IReadOnlyList<NetworkSwitch> ReadSwitches()
    {
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Comment = '#',
            AllowComments = true,
        });

        return csv.GetRecords<SwitchRecord>()
                  .Select(ToSwitch)
                  .ToList()
                  .AsReadOnly();
    }

    private static NetworkSwitch ToSwitch(SwitchRecord r) =>
        new(r.Hostname, r.IpAddress, r.Username, r.Password, r.EnableSecret);
}

internal record SwitchRecord
{
    public string Hostname { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string EnableSecret { get; init; } = string.Empty;
}
