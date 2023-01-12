using Rn.Timerr.Attributes;
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


  // Interface methods
  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
  {
    var jobOutcome = new RunningJobResult(JobOutcome.Failed);

    // Map and validate the job configuration
    var config = RunningJobUtils.MapConfiguration<Config>(options);
    var validationOutcome = RunningJobUtils.ValidateConfig(config);
    if (!validationOutcome.Success)
      return jobOutcome.WithError(validationOutcome.ValidationError);

    // Run job logic...
    var sshClient = await _sshClientFactory.GetSshClient(config.SshCredentials);
    foreach (var folder in config.Folders)
    {
      var directory = Path.GetFileName(folder);
      var destPath = GenerateBackupDestPath(config, directory);

      sshClient.RunCommand($"mkdir -p \"{destPath}\"");
      sshClient.RunCommand($"rm \"{destPath}$(date '+%F')-{directory}.zip\"", false);
      sshClient.RunCommand($"zip -r \"{destPath}$(date '+%F')-{directory}.zip\" \"{folder}\"");
      sshClient.RunCommand($"chmod 0777 \"{destPath}$(date '+%F')-{directory}.zip\"");
    }

    // Reschedule job and return outcome
    options.ScheduleNextRunUsingTemplate(DateTime.Now.AddDays(1), "yyyy-MM-ddT08:20:00.0000000-07:00");
    return jobOutcome.AsSucceeded();
  }


  // Internal methods & supporting classes
  private static string GenerateBackupDestPath(Config config, string directory)
  {
    var generated = Path.Join(config.BackupDestRoot, directory)
      .Replace("\\", "/");

    if (!generated.EndsWith('/'))
      generated += "/";

    return generated;
  }

  private class Config
  {
    [StringArrayConfig("directory")]
    [StringArrayValidator(1)]
    public string[] Folders { get; set; } = Array.Empty<string>();

    [StringConfig("backupDestRoot")]
    [StringValidator]
    public string BackupDestRoot { get; set; } = string.Empty;

    [StringConfig("ssh.creds")]
    [StringValidator]
    public string SshCredentials { get; set; } = string.Empty;
  }
}
