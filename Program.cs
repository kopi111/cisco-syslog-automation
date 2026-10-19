using CiscoSyslogAutomation.Models;
using CiscoSyslogAutomation.Services;
using CiscoSyslogAutomation.Reporting;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var syslogSettings = configuration.GetSection("Syslog").Get<SyslogSettings>()
    ?? throw new InvalidOperationException("Syslog section missing from appsettings.json");

var inventoryPath = configuration["InventoryFile"] ?? "switches.csv";
var maxParallelism = int.Parse(configuration["MaxParallelConnections"] ?? "10");

var inventoryReader = new InventoryReader(inventoryPath);
var switches = inventoryReader.ReadSwitches();

Console.WriteLine("=== Cisco Syslog Automation ===");
Console.WriteLine($"Switches loaded : {switches.Count}");
Console.WriteLine($"Syslog server   : {syslogSettings.ServerIp}");
Console.WriteLine($"Severity        : {syslogSettings.Severity}");
Console.WriteLine($"Parallel limit  : {maxParallelism}");
Console.WriteLine();

var orchestrator = new ConfigurationOrchestrator(syslogSettings, maxParallelism);
var results = await orchestrator.ConfigureAllAsync(switches);

var reporter = new ResultReporter();
reporter.PrintSummary(results);
reporter.SaveReport(results, "syslog_deployment_report.csv");
