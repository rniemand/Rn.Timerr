using System.Data.SqlClient;
using Rn.Timerr.Enums;
using Rn.Timerr.Factories;
using Rn.Timerr.Models;
using RnCore.Logging;

namespace Rn.Timerr.Jobs;

internal class BackupSonarQube : IRunnableJob
{
  public string Name => nameof(BackupSonarQube);
  public string ConfigKey => nameof(BackupSonarQube);

  private readonly ILoggerAdapter<BackupSonarQube> _logger;
  private readonly ISshClientFactory _sshClientFactory;

  public BackupSonarQube(ILoggerAdapter<BackupSonarQube> logger, ISshClientFactory sshClientFactory)
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
    var config = MapConfiguration(options);
    var jobOutcome = new RunningJobResult(JobOutcome.Failed);

    if (!config.IsValid())
      return jobOutcome.WithError("Missing required configuration");

    await CreateDbBackupAsync(config);
    await ProcessDbBackup(config);
    ScheduleNextRunTime(options);

    return jobOutcome.AsSucceeded();
  }


  // Internal methods
  private static BackupSonarQubeConfig MapConfiguration(RunningJobOptions options) =>
    new()
    {
      SqlConnectionString = options.Config.GetStringValue("SqlConnection"),
      SshConnectionName = options.Config.GetStringValue("ssh.creds")
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

  private async Task ProcessDbBackup(BackupSonarQubeConfig config)
  {
    var sshClient = await _sshClientFactory.GetSshClient(config.SshConnectionName);

    sshClient.RunSshCommand("chmod 0777 /mnt/user/appdata/sql-server/data/SonarQube.bak");
    sshClient.RunSshCommand("mv /mnt/user/appdata/sql-server/data/SonarQube.bak /mnt/user/Backups/db-mssql/SonarQube.bak");
    sshClient.RunSshCommand("rm \"/mnt/user/Backups/db-mssql/$(date '+%F')-SonarQube.zip\"", false);
    sshClient.RunSshCommand("zip -r \"/mnt/user/Backups/db-mssql/$(date '+%F')-SonarQube.zip\" \"/mnt/user/Backups/db-mssql/SonarQube.bak\"");
    sshClient.RunSshCommand("rm /mnt/user/Backups/db-mssql/SonarQube.bak");
  }

  private void ScheduleNextRunTime(RunningJobOptions options)
  {
    var nextRunTime = DateTimeOffset.Now.AddHours(23);
    options.State.SetValue("NextRunTime", nextRunTime);
    _logger.LogInformation("Scheduled next run time for: {time}", nextRunTime);
  }
}

class BackupSonarQubeConfig
{
  public string SqlConnectionString { get; set; } = string.Empty;
  public string SshConnectionName { get; set; } = string.Empty;

  public bool IsValid()
  {
    if (string.IsNullOrWhiteSpace(SqlConnectionString))
      return false;

    if (string.IsNullOrWhiteSpace(SshConnectionName))
      return false;

    return true;
  }
}
