using CiscoSyslogAutomation.Models;

namespace CiscoSyslogAutomation.Services;

public class SwitchConfigurator
{
    private readonly SyslogSettings _syslogSettings;

    public SwitchConfigurator(SyslogSettings syslogSettings)
    {
        _syslogSettings = syslogSettings;
    }

    public ConfigurationResult Configure(NetworkSwitch sw)
    {
        try
        {
            ApplySyslogConfiguration(sw);
            return new ConfigurationResult(sw.Hostname, sw.IpAddress, Success: true);
        }
        catch (Exception ex)
        {
            return new ConfigurationResult(sw.Hostname, sw.IpAddress, Success: false, ex.Message);
        }
    }

    private void ApplySyslogConfiguration(NetworkSwitch sw)
    {
        using var session = CiscoShellSession.Connect(sw.IpAddress, sw.Username, sw.Password);
        session.EnterEnableMode(sw.EnableSecret);
        session.EnterConfigMode();

        foreach (var command in BuildSyslogCommands())
            session.SendConfigCommand(command);

        session.ExitConfigMode();
        session.SaveConfig();
    }

    private IEnumerable<string> BuildSyslogCommands() =>
    [
        $"logging host {_syslogSettings.ServerIp}",
        $"logging trap {_syslogSettings.Severity}",
        "service timestamps log datetime msec localtime",
        $"logging source-interface {_syslogSettings.SourceInterface}",
        "logging on",
    ];
}
