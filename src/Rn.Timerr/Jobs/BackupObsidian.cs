using Rn.Timerr.Enums;
using Rn.Timerr.Factories;
using Rn.Timerr.Models;
using RnCore.Logging;

namespace Rn.Timerr.Jobs;

internal class BackupObsidian : IRunnableJob
{
  public string Name => nameof(BackupObsidian);
  public string ConfigKey => nameof(BackupObsidian);

  private readonly ILoggerAdapter<BackupObsidian> _logger;
  private readonly ISshClientFactory _sshClientFactory;

  public BackupObsidian(ILoggerAdapter<BackupObsidian> logger, ISshClientFactory sshClientFactory)
  {
    _logger = logger;
    _sshClientFactory = sshClientFactory;
  }

  // Interface methods
  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
  {
    var config = MapConfiguration(options);
    var jobOutcome = new RunningJobResult(JobOutcome.Failed);

    if (!config.IsValid())
      return jobOutcome.WithError("Missing required configuration");

    // Execute the backup commands
    var sshClient = await _sshClientFactory.GetSshClient(config.SshCredentials);
    sshClient.RunCommand("rm \"/mnt/user/Backups/Obsidian/$(date '+%F')-Obsidian.zip\"", false);
    sshClient.RunCommand("zip -r \"/mnt/user/Backups/Obsidian/$(date '+%F')-Obsidian.zip\" \"/mnt/user/Backups/_Obsidian/\"");

    // Schedule the next run time
    ScheduleNextRunTime(options);
    await Task.CompletedTask;

    return jobOutcome.AsSucceeded();
  }


  // Internal methods
  private static BackupObsidianConfig MapConfiguration(RunningJobOptions options) =>
    new()
    {
      SshCredentials = options.Config.GetStringValue("ssh.creds")
    };

  private void ScheduleNextRunTime(RunningJobOptions options)
  {
    var nextRunTime = DateTimeOffset.Now.AddHours(12);
    options.State.SetValue("NextRunTime", nextRunTime);
    _logger.LogInformation("Scheduled next run time for: {time}", nextRunTime);
  }
}

class BackupObsidianConfig
{
  public string SshCredentials { get; set; } = string.Empty;

  public bool IsValid() => !string.IsNullOrWhiteSpace(SshCredentials);
}
