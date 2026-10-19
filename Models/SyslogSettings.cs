namespace CiscoSyslogAutomation.Models;

public record SyslogSettings(
    string ServerIp,
    string Severity,
    string SourceInterface);
