using System.Data.SqlClient;
using Renci.SshNet;
using Rn.Timerr.Enums;
using Rn.Timerr.Exceptions;
using Rn.Timerr.Models;
using Rn.Timerr.Services;
using RnCore.Logging;

namespace Rn.Timerr.Jobs;

internal class BackupSonarQube : IRunnableJob
{
  public string Name => nameof(BackupSonarQube);
  public string ConfigKey => nameof(BackupSonarQube);

  private readonly ILoggerAdapter<BackupSonarQube> _logger;
  private readonly ICredentialsService _credentialsService;

  public BackupSonarQube(ILoggerAdapter<BackupSonarQube> logger,
    ICredentialsService credentialsService)
  {
    _logger = logger;
    _credentialsService = credentialsService;
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
    var config = await MapConfiguration(options);
    var jobOutcome = new RunningJobResult(JobOutcome.Failed);

    if (!config.IsValid())
      return jobOutcome.WithError("Missing required configuration");

    await CreateDbBackupAsync(config);
    ProcessDbBackup(config);
    ScheduleNextRunTime(options);

    return jobOutcome.AsSucceeded();
  }


  // Internal methods
  private async Task<BackupSonarQubeConfig> MapConfiguration(RunningJobOptions options)
  {
    var credentialsName = options.Config.GetStringValue("ssh.creds");
    if (string.IsNullOrWhiteSpace(credentialsName))
    {
      _logger.LogError("Missing required config value: {name}", "ssh.creds");
      return new BackupSonarQubeConfig();
    }

    return new BackupSonarQubeConfig
    {
      SqlConnectionString = options.Config.GetStringValue("SqlConnection"),
      SshCredentials = await _credentialsService.GetSshCredentials(credentialsName)
    };
  }

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
    var sshClient = GetSshClient(config);

    RunSshCommand(sshClient, "chmod 0777 /mnt/user/appdata/sql-server/data/SonarQube.bak");
    RunSshCommand(sshClient, "mv /mnt/user/appdata/sql-server/data/SonarQube.bak /mnt/user/Backups/db-mssql/SonarQube.bak");
    RunSshCommand(sshClient, "rm \"/mnt/user/Backups/db-mssql/$(date '+%F')-SonarQube.zip\"", false);
    RunSshCommand(sshClient, "zip -r \"/mnt/user/Backups/db-mssql/$(date '+%F')-SonarQube.zip\" \"/mnt/user/Backups/db-mssql/SonarQube.bak\"");
    RunSshCommand(sshClient, "rm /mnt/user/Backups/db-mssql/SonarQube.bak");
  }

  private void ScheduleNextRunTime(RunningJobOptions options)
  {
    var nextRunTime = DateTimeOffset.Now.AddHours(23);
    options.State.SetValue("NextRunTime", nextRunTime);
    _logger.LogInformation("Scheduled next run time for: {time}", nextRunTime);
  }

  private static SshClient GetSshClient(BackupSonarQubeConfig config)
  {
    var credentials = new PasswordAuthenticationMethod(config.SshCredentials.User, config.SshCredentials.Pass);
    var sshClient = new SshClient(new ConnectionInfo(config.SshCredentials.Host, config.SshCredentials.Port, config.SshCredentials.User, credentials));
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
    throw new RnTimerrException($"ssh command error: {commandOutput.Error}");
  }
}

class BackupSonarQubeConfig
{
  public string SqlConnectionString { get; set; } = string.Empty;
  public SshCredentials SshCredentials { get; set; } = new();

  public bool IsValid()
  {
    if (string.IsNullOrWhiteSpace(SqlConnectionString))
      return false;

    if (!SshCredentials.IsValid())
      return false;

    return true;
  }
}
