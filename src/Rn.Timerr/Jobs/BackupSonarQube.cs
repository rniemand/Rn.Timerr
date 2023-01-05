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
    await CreateDbBackupAsync(options);
    ProcessDbBackup(options);
    ScheduleNextRunTime(options);

    return new RunningJobResult(JobOutcome.Succeeded);
  }


  // Internal methods
  private async Task CreateDbBackupAsync(RunningJobOptions options)
  {
    var sqlConnectionString = options.Config.GetStringValue("SqlConnection");
    if (string.IsNullOrWhiteSpace(sqlConnectionString))
      throw new Exception("Unable to find SQL connection string");

    var sqlConnection = new SqlConnection(sqlConnectionString);
    sqlConnection.Open();

    const string backupQuery = @"BACKUP DATABASE SonarQube 
    TO DISK = '/var/opt/mssql/data/SonarQube.bak'";

    _logger.LogInformation("Backing up SonarQube DB");
    var sqlCommand = new SqlCommand(backupQuery, sqlConnection);
    sqlCommand.CommandTimeout = 0;
    await sqlCommand.ExecuteNonQueryAsync();
    _logger.LogInformation("   ... backup completed");
  }

  private void ProcessDbBackup(RunningJobOptions options)
  {
    var client = GetSshClient(options);

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
    _logger.LogDebug("Scheduled next tick for: {time}", nextRunTime);
  }

  private static SshClient GetSshClient(RunningJobOptions options)
  {
    var host = options.Config.GetStringValue("ssh.host");
    var port = options.Config.GetIntValue("ssh.port", 22);
    var user = options.Config.GetStringValue("ssh.user");
    var pass = options.Config.GetStringValue("ssh.pass");

    var creds = new PasswordAuthenticationMethod(user, pass);
    var sshClient = new SshClient(new ConnectionInfo(host, port, user, creds));
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
