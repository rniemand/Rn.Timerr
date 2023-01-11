using Rn.Timerr.Enums;
using Rn.Timerr.Factories;
using Rn.Timerr.Models;
using Rn.Timerr.Utils;

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
    var jobOutcome = new RunningJobResult(JobOutcome.Failed);

    var config = RunningJobUtils.MapConfiguration<BackupObsidianConfig>(options);
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
}

class BackupObsidianConfig
{
  [JobDbConfig("ssh.creds")]
  public string SshCredentials { get; set; } = string.Empty;

  public bool IsValid() => !string.IsNullOrWhiteSpace(SshCredentials);
}
