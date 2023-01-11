using Rn.Timerr.Enums;
using Rn.Timerr.Factories;
using Rn.Timerr.Models;
using Rn.Timerr.Utils;

namespace Rn.Timerr.Jobs;

class BackupAppData : IRunnableJob
{
  public string Name => nameof(BackupAppData);
  public string ConfigKey => nameof(BackupAppData);

  private readonly ISshClientFactory _sshClientFactory;

  public BackupAppData(ISshClientFactory sshClientFactory)
  {
    _sshClientFactory = sshClientFactory;
  }

  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
  {
    var jobOutcome = new RunningJobResult(JobOutcome.Failed);
    var config = RunningJobUtils.MapConfiguration<Config>(options);

    if (!config.IsValid())
      return jobOutcome.WithError("Missing required configuration");

    var sshClient = await _sshClientFactory.GetSshClient(config.SshCredsName);

    foreach (var folder in config.Folders)
    {
      var directory = Path.GetFileName(folder);
      var destPath = GenerateBackupDestPath(config, directory);

      sshClient.RunCommand($"mkdir -p \"{destPath}\"");
      sshClient.RunCommand($"rm \"{destPath}$(date '+%F')-{directory}.zip\"", false);
      sshClient.RunCommand($"zip -r \"{destPath}$(date '+%F')-{directory}.zip\" \"{folder}\"");
      sshClient.RunCommand($"chmod 0777 \"{destPath}$(date '+%F')-{directory}.zip\"");
    }

    options.ScheduleNextRunUsingTemplate(DateTime.Now.AddDays(1), "yyyy-MM-ddT08:20:00.0000000-07:00");
    return jobOutcome.AsSucceeded();
  }


  // Internal methods
  private static string GenerateBackupDestPath(Config config, string directory)
  {
    var generated = Path.Join(config.BackupDestRoot, directory)
      .Replace("\\", "/");

    if (!generated.EndsWith('/'))
      generated += "/";

    return generated;
  }


  // Supporting Classes
  class Config
  {
    [JobDbConfig("directory", JobDbConfigType.StringArray)]
    public List<string> Folders { get; set; } = new();

    [JobDbConfig("backupDestRoot")]
    public string BackupDestRoot { get; set; } = string.Empty;

    [JobDbConfig("ssh.creds")]
    public string SshCredsName { get; set; } = string.Empty;

    public bool IsValid()
    {
      if (string.IsNullOrWhiteSpace(BackupDestRoot))
        return false;

      if (string.IsNullOrWhiteSpace(SshCredsName))
        return false;

      if (Folders.Count == 0)
        return false;

      return true;
    }
  }
}
