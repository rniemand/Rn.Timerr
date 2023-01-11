using Rn.Timerr.Enums;
using Rn.Timerr.Factories;
using Rn.Timerr.Models;

namespace Rn.Timerr.Jobs;

internal class BackupObsidian : IRunnableJob
{
  public string Name => nameof(BackupObsidian);
  public string ConfigKey => nameof(BackupObsidian);

  private readonly ISshClientFactory _sshClientFactory;

  public BackupObsidian(ISshClientFactory sshClientFactory)
  {
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
    options.ScheduleNextRunInXHours(12);
    return jobOutcome.AsSucceeded();
  }


  // Internal methods
  private static BackupObsidianConfig MapConfiguration(RunningJobOptions options) =>
    new()
    {
      SshCredentials = options.Config.GetStringValue("ssh.creds")
    };
}

class BackupObsidianConfig
{
  public string SshCredentials { get; set; } = string.Empty;

  public bool IsValid() => !string.IsNullOrWhiteSpace(SshCredentials);
}
