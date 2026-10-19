using Renci.SshNet;
using System.Text.RegularExpressions;

namespace CiscoSyslogAutomation.Services;

public class CiscoShellSession : IDisposable
{
    private static readonly Regex ExecPromptPattern = new(@"[>#]", RegexOptions.Multiline);
    private static readonly Regex ConfigPromptPattern = new(@"\(config\)#");
    private static readonly Regex PasswordOrPromptPattern = new(@"[Pp]assword:|[>#]");
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan SaveTimeout = TimeSpan.FromSeconds(30);

    private readonly SshClient _client;
    private readonly ShellStream _shell;
    private bool _isDisposed;

    private CiscoShellSession(SshClient client, ShellStream shell)
    {
        _client = client;
        _shell = shell;
    }

    public static CiscoShellSession Connect(string host, string username, string password)
    {
        var auth = new PasswordAuthenticationMethod(username, password);
        var connectionInfo = new ConnectionInfo(host, username, auth);

        var client = new SshClient(connectionInfo);
        client.Connect();

        var shell = client.CreateShellStream("vt100", 80, 24, 800, 600, 1024);
        var session = new CiscoShellSession(client, shell);
        session.WaitForExecPrompt();

        return session;
    }

    public void EnterEnableMode(string enableSecret)
    {
        _shell.WriteLine("enable");
        var response = _shell.Expect(PasswordOrPromptPattern, CommandTimeout) ?? string.Empty;

        if (ContainsPasswordPrompt(response))
        {
            _shell.WriteLine(enableSecret);
            WaitForExecPrompt();
        }
    }

    public void EnterConfigMode()
    {
        _shell.WriteLine("configure terminal");
        _shell.Expect(ConfigPromptPattern, CommandTimeout);
    }

    public void SendConfigCommand(string command)
    {
        _shell.WriteLine(command);
        _shell.Expect(ConfigPromptPattern, CommandTimeout);
    }

    public void ExitConfigMode()
    {
        _shell.WriteLine("end");
        WaitForExecPrompt();
    }

    public void SaveConfig()
    {
        _shell.WriteLine("write memory");
        _shell.Expect(ExecPromptPattern, SaveTimeout);
    }

    private void WaitForExecPrompt() => _shell.Expect(ExecPromptPattern, CommandTimeout);

    private static bool ContainsPasswordPrompt(string response) =>
        response.Contains("Password", StringComparison.OrdinalIgnoreCase);

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _shell.Dispose();
        _client.Disconnect();
        _client.Dispose();
    }
}
