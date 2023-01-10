using Renci.SshNet;
using Rn.Timerr.Enums;
using Rn.Timerr.Exceptions;
using Rn.Timerr.Models;
using Rn.Timerr.Services;
using RnCore.Logging;

namespace Rn.Timerr.Jobs;

class BackupAppData : IRunnableJob
{
  public string Name => nameof(BackupAppData);
  public string ConfigKey => nameof(BackupAppData);

  private readonly ILoggerAdapter<BackupAppData> _logger;
  private readonly ICredentialsService _credentialsService;

  public BackupAppData(ILoggerAdapter<BackupAppData> logger,
    ICredentialsService credentialsService)
  {
    _logger = logger;
    _credentialsService = credentialsService;
  }


  // Interface methods
  public bool CanRun(RunningJobOptions options)
  {
    if (!options.State.ContainsKey("NextRunTime"))
      return true;

    var nextRunTime = options.State.GetDateTimeOffsetValue("NextRunTime");

    if (nextRunTime > options.JobStartTime)
      return false;

    _logger.LogDebug("Current time {now} > {runTime} (Running Job)", options.JobStartTime, nextRunTime);
    return true;
  }

  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
  {
    var jobOutcome = new RunningJobResult(JobOutcome.Failed);
    var config = await MapConfiguration(options);

    if (!config.IsValid())
      return jobOutcome.WithError("Missing required configuration");

    var sshClient = GetSshClient(config);

    foreach (var folder in config.Folders)
    {
      var directory = Path.GetFileName(folder);
      var destPath = GenerateBackupDestPath(config, directory);

      RunSshCommand(sshClient, $"mkdir -p \"{destPath}\"");
      RunSshCommand(sshClient, $"rm \"{destPath}$(date '+%F')-{directory}.zip\"", false);
      RunSshCommand(sshClient, $"zip -r \"{destPath}$(date '+%F')-{directory}.zip\" \"{folder}\"");
      RunSshCommand(sshClient, $"chmod 0777 \"{destPath}$(date '+%F')-{directory}.zip\"");
    }

    ScheduleNextRunTime(options);
    await Task.CompletedTask;

    return jobOutcome.AsSucceeded();
  }


  // Internal methods
  private async Task<BackupAppDataConfig> MapConfiguration(RunningJobOptions options)
  {
    var credentialsName = options.Config.GetStringValue("ssh.creds");
    if (string.IsNullOrWhiteSpace(credentialsName))
    {
      _logger.LogError("Missing required config value: {name}", "ssh.creds");
      return new BackupAppDataConfig();
    }

    return new BackupAppDataConfig
    {
      Folders = options.Config.GetStringCollection("directory"),
      BackupDestRoot = options.Config.GetStringValue("backupDestRoot"),
      SshCredentials = await _credentialsService.GetSshCredentials(credentialsName)
    };
  }

  private static SshClient GetSshClient(BackupAppDataConfig config)
  {
    var credentials = new PasswordAuthenticationMethod(config.SshCredentials.User, config.SshCredentials.Pass);
    var sshClient = new SshClient(new ConnectionInfo(config.SshCredentials.Host, config.SshCredentials.Port, config.SshCredentials.User, credentials));
    sshClient.Connect();
    return sshClient;
  }

  private static string GenerateBackupDestPath(BackupAppDataConfig config, string directory)
  {
    var generated = Path.Join(config.BackupDestRoot, directory)
      .Replace("\\", "/");

    if (!generated.EndsWith('/'))
      generated += "/";

    return generated;
  }

  private void RunSshCommand(SshClient client, string commandText, bool throwOnError = true)
  {
    _logger.LogDebug("Running SSH command: {cmd}", commandText);

    var commandOutput = client.RunCommand(commandText);

    if (string.IsNullOrWhiteSpace(commandOutput.Error) || !throwOnError)
      return;

    _logger.LogError("Error running command '{cmd}': {error}", commandOutput, commandOutput.Error);
    throw new RnTimerrException($"ssh command error: {commandOutput.Error}");
  }

  private void ScheduleNextRunTime(RunningJobOptions options)
  {
    var tomorrow = DateTime.Now.AddDays(1);
    var nextRunTime = DateTimeOffset.Parse("yyyy-MM-ddT08:20:00.0000000-07:00"
      .Replace("yyyy", tomorrow.Year.ToString())
      .Replace("MM", tomorrow.Month.ToString().PadLeft(2, '0'))
      .Replace("dd", tomorrow.Day.ToString().PadLeft(2, '0')));

    options.State.SetValue("NextRunTime", nextRunTime);
    _logger.LogInformation("Scheduled next run time for: {time}", nextRunTime);
  }
}

class BackupAppDataConfig
{
  public List<string> Folders { get; set; } = new();
  public string BackupDestRoot { get; set; } = string.Empty;
  public SshCredentials SshCredentials { get; set; } = new();

  public bool IsValid()
  {
    if (string.IsNullOrWhiteSpace(BackupDestRoot))
      return false;

    if (Folders.Count == 0)
      return false;

    if (!SshCredentials.IsValid())
      return false;

    return true;
  }
}
