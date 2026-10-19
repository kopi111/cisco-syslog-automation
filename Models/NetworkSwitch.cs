namespace CiscoSyslogAutomation.Models;

public record NetworkSwitch(
    string Hostname,
    string IpAddress,
    string Username,
    string Password,
    string EnableSecret);
