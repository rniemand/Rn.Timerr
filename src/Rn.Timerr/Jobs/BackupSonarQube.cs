using System.Data.SqlClient;
using Renci.SshNet;
using Rn.Timerr.Enums;
using Rn.Timerr.Models;
using RnCore.Logging;

namespace Rn.Timerr.Jobs;

internal class BackupSonarQube : IRunnableJob
{
  public string Name => nameof(BackupSonarQube);
  public string ConfigKey => nameof(BackupSonarQube);

  private readonly ILoggerAdapter<BackupSonarQube> _logger;

  public BackupSonarQube(ILoggerAdapter<BackupSonarQube> logger)
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

    await CreateDbBackupAsync(config);
    ProcessDbBackup(config);
    ScheduleNextRunTime(options);

    return jobOutcome.AsSucceeded();
  }


  // Internal methods
  private BackupSonarQubeConfig MapConfiguration(RunningJobOptions options) =>
    new()
    {
      SqlConnectionString = options.Config.GetStringValue("SqlConnection"),
      SshHost = options.Config.GetStringValue("ssh.host"),
      SshPort = options.Config.GetIntValue("ssh.port", 22),
      SshUser = options.Config.GetStringValue("ssh.user"),
      SshPass = options.Config.GetStringValue("ssh.pass")
    };

  private async Task CreateDbBackupAsync(BackupSonarQubeConfig config)
  {
    var sqlConnection = new SqlConnection(config.SqlConnectionString);
    sqlConnection.Open();

    const string backupQuery = @"BACKUP DATABASE SonarQube 
    TO DISK = '/var/opt/mssql/data/SonarQube.bak'";

    _logger.LogInformation("Backing up SonarQube DB");
    var sqlCommand = new SqlCommand(backupQuery, sqlConnection);
    sqlCommand.CommandTimeout = 0;
    await sqlCommand.ExecuteNonQueryAsync();
    _logger.LogInformation("   ... backup completed");
  }

  private void ProcessDbBackup(BackupSonarQubeConfig config)
  {
    var client = GetSshClient(config);

    RunSshCommand(client, "chmod 0777 /mnt/user/appdata/sql-server/data/SonarQube.bak");
    RunSshCommand(client, "mv /mnt/user/appdata/sql-server/data/SonarQube.bak /mnt/user/Backups/db-mssql/SonarQube.bak");
    RunSshCommand(client, "rm \"/mnt/user/Backups/db-mssql/$(date '+%F')-SonarQube.zip\"", false);
    RunSshCommand(client, "zip -r \"/mnt/user/Backups/db-mssql/$(date '+%F')-SonarQube.zip\" \"/mnt/user/Backups/db-mssql/SonarQube.bak\"");
    RunSshCommand(client, "rm /mnt/user/Backups/db-mssql/SonarQube.bak");
  }

  private void ScheduleNextRunTime(RunningJobOptions options)
  {
    var nextRunTime = DateTimeOffset.Now.AddHours(23);
    options.State.SetValue("NextRunTime", nextRunTime);
    _logger.LogInformation("Scheduled next run time for: {time}", nextRunTime);
  }

  private static SshClient GetSshClient(BackupSonarQubeConfig config)
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
}

class BackupSonarQubeConfig
{
  public string SqlConnectionString { get; set; } = string.Empty;
  public string SshHost { get; set; } = string.Empty;
  public int SshPort { get; set; } = 22;
  public string SshUser { get; set; } = string.Empty;
  public string SshPass { get; set; } = string.Empty;

  public bool IsValid()
  {
    if (string.IsNullOrWhiteSpace(SqlConnectionString))
      return false;

    if (string.IsNullOrWhiteSpace(SshHost))
      return false;

    if (string.IsNullOrWhiteSpace(SshUser))
      return false;

    if (string.IsNullOrWhiteSpace(SshPass))
      return false;

    return true;
  }
}
