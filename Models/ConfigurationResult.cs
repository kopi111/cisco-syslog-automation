namespace CiscoSyslogAutomation.Models;

public record ConfigurationResult(
    string Hostname,
    string IpAddress,
    bool Success,
    string? ErrorMessage = null)
{
    public string Status => Success ? "OK" : "FAILED";
}
