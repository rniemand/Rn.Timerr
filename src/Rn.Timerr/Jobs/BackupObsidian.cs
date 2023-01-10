using Renci.SshNet;
using Rn.Timerr.Enums;
using Rn.Timerr.Exceptions;
using Rn.Timerr.Models;
using Rn.Timerr.Services;
using RnCore.Logging;

namespace Rn.Timerr.Jobs;

internal class BackupObsidian : IRunnableJob
{
  public string Name => nameof(BackupObsidian);
  public string ConfigKey => nameof(BackupObsidian);

  private readonly ILoggerAdapter<BackupObsidian> _logger;
  private readonly ICredentialsService _credentialsService;


  public BackupObsidian(ILoggerAdapter<BackupObsidian> logger,
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
    var config = await MapConfiguration(options);
    var jobOutcome = new RunningJobResult(JobOutcome.Failed);

    if (!config.IsValid())
      return jobOutcome.WithError("Missing required configuration");

    // Execute the backup commands
    var sshClient = GetSshClient(config);
    RunSshCommand(sshClient, "rm \"/mnt/user/Backups/Obsidian/$(date '+%F')-Obsidian.zip\"", false);
    RunSshCommand(sshClient, "zip -r \"/mnt/user/Backups/Obsidian/$(date '+%F')-Obsidian.zip\" \"/mnt/user/Backups/_Obsidian/\"");

    // Schedule the next run time
    ScheduleNextRunTime(options);
    await Task.CompletedTask;

    return jobOutcome.AsSucceeded();
  }


  // Internal methods
  private async Task<BackupObsidianConfig> MapConfiguration(RunningJobOptions options)
  {
    var credentialsName = options.Config.GetStringValue("ssh.creds");
    if (string.IsNullOrWhiteSpace(credentialsName))
    {
      _logger.LogError("Missing required config value: {name}", "ssh.creds");
      return new BackupObsidianConfig();
    }

    return new BackupObsidianConfig
    {
      SshCredentials = await _credentialsService.GetSshCredentials(credentialsName)
    };
  }

  private static SshClient GetSshClient(BackupObsidianConfig config)
  {
    var credentials = new PasswordAuthenticationMethod(config.SshCredentials.User, config.SshCredentials.Pass);
    var sshClient = new SshClient(new ConnectionInfo(config.SshCredentials.Host, config.SshCredentials.Port, config.SshCredentials.User, credentials));
    sshClient.Connect();
    return sshClient;
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
    var nextRunTime = DateTimeOffset.Now.AddHours(12);
    options.State.SetValue("NextRunTime", nextRunTime);
    _logger.LogInformation("Scheduled next run time for: {time}", nextRunTime);
  }
}

class BackupObsidianConfig
{
  public SshCredentials SshCredentials { get; set; } = new();

  public bool IsValid()
  {
    return SshCredentials.IsValid();
  }
}
