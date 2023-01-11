using System.Data.SqlClient;
using Rn.Timerr.Enums;
using Rn.Timerr.Factories;
using Rn.Timerr.Models;
using Rn.Timerr.Utils;
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
  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
  {
    var config = RunningJobUtils.MapConfiguration<BackupSonarQubeConfig>(options);
    var jobOutcome = new RunningJobResult(JobOutcome.Failed);

    if (!config.IsValid())
      return jobOutcome.WithError("Missing required configuration");

    await CreateDbBackupAsync(config);
    await ProcessDbBackup(config);
    options.ScheduleNextRunUsingTemplate(DateTime.Now.AddDays(1), "yyyy-MM-ddT08:10:00.0000000-07:00");

    return jobOutcome.AsSucceeded();
  }


  // Internal methods
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

    sshClient.RunCommand("chmod 0777 /mnt/user/appdata/sql-server/data/SonarQube.bak");
    sshClient.RunCommand("mv /mnt/user/appdata/sql-server/data/SonarQube.bak /mnt/user/Backups/db-mssql/SonarQube.bak");
    sshClient.RunCommand("rm \"/mnt/user/Backups/db-mssql/$(date '+%F')-SonarQube.zip\"", false);
    sshClient.RunCommand("zip -r \"/mnt/user/Backups/db-mssql/$(date '+%F')-SonarQube.zip\" \"/mnt/user/Backups/db-mssql/SonarQube.bak\"");
    sshClient.RunCommand("rm /mnt/user/Backups/db-mssql/SonarQube.bak");
  }
}

class BackupSonarQubeConfig
{
  [JobDbConfig("SqlConnection")]
  public string SqlConnectionString { get; set; } = string.Empty;

  [JobDbConfig("ssh.creds")]
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
