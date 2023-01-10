using Rn.Timerr.Enums;
using Rn.Timerr.Factories;
using Rn.Timerr.Models;
using RnCore.Logging;

namespace Rn.Timerr.Jobs;

class BackupAppData : IRunnableJob
{
  public string Name => nameof(BackupAppData);
  public string ConfigKey => nameof(BackupAppData);

  private readonly ILoggerAdapter<BackupAppData> _logger;
  private readonly ISshClientFactory _sshClientFactory;

  public BackupAppData(ILoggerAdapter<BackupAppData> logger, ISshClientFactory sshClientFactory)
  {
    _logger = logger;
    _sshClientFactory = sshClientFactory;
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
    var config = MapConfiguration(options);

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

    ScheduleNextRunTime(options);
    return jobOutcome.AsSucceeded();
  }


  // Internal methods
  private static BackupAppDataConfig MapConfiguration(RunningJobOptions options) =>
    new()
    {
      Folders = options.Config.GetStringCollection("directory"),
      BackupDestRoot = options.Config.GetStringValue("backupDestRoot"),
      SshCredsName = options.Config.GetStringValue("ssh.creds")
    };

  private static string GenerateBackupDestPath(BackupAppDataConfig config, string directory)
  {
    var generated = Path.Join(config.BackupDestRoot, directory)
      .Replace("\\", "/");

    if (!generated.EndsWith('/'))
      generated += "/";

    return generated;
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
