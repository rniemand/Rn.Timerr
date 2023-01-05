using Renci.SshNet;
using Rn.Timerr.Enums;
using Rn.Timerr.Models;
using RnCore.Logging;

namespace Rn.Timerr.Jobs;

internal class BackupObsidian : IRunnableJob
{
  public string Name => nameof(BackupObsidian);
  public string ConfigKey => nameof(BackupObsidian);

  private readonly ILoggerAdapter<BackupObsidian> _logger;


  public BackupObsidian(ILoggerAdapter<BackupObsidian> logger)
  {
    _logger = logger;
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
    var config = MapConfiguration(options);
    var jobOutcome = new RunningJobResult(JobOutcome.Failed);

    if (!config.IsValid())
      return jobOutcome.WithError("Missing required configuration");

    // Execute the backup commands
    var client = GetSshClient(config);
    RunSshCommand(client, "rm \"/mnt/user/Backups/Obsidian/$(date '+%F')-Obsidian.zip\"", false);
    RunSshCommand(client, "zip -r \"/mnt/user/Backups/Obsidian/$(date '+%F')-Obsidian.zip\" \"/mnt/user/Backups/_Obsidian/\"");

    // Schedule the next run time
    ScheduleNextRunTime(options);
    await Task.CompletedTask;

    return jobOutcome.AsSucceeded();
  }


  // Internal methods
  private BackupObsidianConfig MapConfiguration(RunningJobOptions options) =>
    new()
    {
      SshHost = options.Config.GetStringValue("ssh.host"),
      SshPort = options.Config.GetIntValue("ssh.port", 22),
      SshUser = options.Config.GetStringValue("ssh.user"),
      SshPass = options.Config.GetStringValue("ssh.pass")
    };

  private static SshClient GetSshClient(BackupObsidianConfig config)
  {
    var credentials = new PasswordAuthenticationMethod(config.SshUser, config.SshPass);
    var sshClient = new SshClient(new ConnectionInfo(config.SshHost, config.SshPort, config.SshUser, credentials));
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
    throw new Exception($"ssh command error: {commandOutput.Error}");
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
  public string SshHost { get; set; } = string.Empty;
  public int SshPort { get; set; } = 22;
  public string SshUser { get; set; } = string.Empty;
  public string SshPass { get; set; } = string.Empty;

  public bool IsValid()
  {
    if (string.IsNullOrWhiteSpace(SshHost))
      return false;

    if (string.IsNullOrWhiteSpace(SshUser))
      return false;

    if (string.IsNullOrWhiteSpace(SshPass))
      return false;

    return true;
  }
}
