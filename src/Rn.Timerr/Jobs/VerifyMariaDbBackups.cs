using Newtonsoft.Json;
using Rn.Timerr.Enums;
using Rn.Timerr.Models;
using RnCore.Logging;
using RnCore.Mailer.Builders;
using RnCore.Mailer.Config;
using RnCore.Mailer.Factories;

namespace Rn.Timerr.Jobs;

class VerifyMariaDbBackups : IRunnableJob
{
  public string Name => nameof(VerifyMariaDbBackups);
  public string ConfigKey => nameof(VerifyMariaDbBackups);

  private readonly ILoggerAdapter<VerifyMariaDbBackups> _logger;
  private readonly IMailTemplateHelper _mailTemplateHelper;
  private readonly IRnMailUtilsFactory _mailUtilsFactory;
  private readonly RnMailConfig _mailConfig;

  public VerifyMariaDbBackups(ILoggerAdapter<VerifyMariaDbBackups> logger,
    IMailTemplateHelper mailTemplateHelper,
    RnMailConfig mailConfig,
    IRnMailUtilsFactory mailUtilsFactory)
  {
    _logger = logger;
    _mailTemplateHelper = mailTemplateHelper;
    _mailConfig = mailConfig;
    _mailUtilsFactory = mailUtilsFactory;
  }


  // Interface methods
  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
  {
    var outcome = new RunningJobResult(JobOutcome.Failed);

    var config = MapConfig(options);
    if (!config.IsValid())
      return outcome.WithError("Missing required configuration");

    var checkConfig = GetDbCheckConfig(config);
    if (checkConfig.Rules.Length == 0)
      return outcome.WithError($"Unable to find config file: {config.ConfigFile}");

    foreach (var rule in checkConfig.Rules)
    {
      // Ensure that the backup file exists
      if (!File.Exists(rule.FilePath))
      {
        await SendFileNotFoundEmail(checkConfig, rule);
        continue;
      }

      // Ensure that the backup file is above the min size threshold
      var backupFileInfo = new FileInfo(rule.FilePath);
      if (backupFileInfo.Length >= rule.MinFileSizeBytes)
        continue;

      await SendBackupFileSizeTooSmallEmail(checkConfig, rule, backupFileInfo);
    }

    await SendCheckCompleteEmail(checkConfig);
    ScheduleNextRunTime(options, config);
    return outcome.AsSucceeded();
  }


  // Internal methods
  private static VerifyMariaDbBackupsConfig MapConfig(RunningJobOptions options) => new()
  {
    ConfigFile = options.Config.GetStringValue("configFile"),
    NextRunTemplate = options.Config.GetStringValue("NextRunTemplate"),
  };

  private static DbBackupVerifyConfig GetDbCheckConfig(VerifyMariaDbBackupsConfig config)
  {
    // TODO: [ABSTRACT] (VerifyMariaDbBackups.GetDbCheckConfig) Use abstraction for this
    if (!File.Exists(config.ConfigFile))
      return new DbBackupVerifyConfig();

    var rawJson = File.ReadAllText(config.ConfigFile);
    var parsedConfig = JsonConvert.DeserializeObject<DbBackupVerifyConfig>(rawJson);

    foreach (var rule in parsedConfig!.Rules)
    {
      rule.FilePath = TemplateStringParser.Parse(rule.FilePath);
      rule.MinFileSizeBytes = rule.MinFileSizeKb * 1024;
    }

    return parsedConfig;
  }

  private async Task SendFileNotFoundEmail(DbBackupVerifyConfig config, DbBackupVerifyConfig.CheckRule rule)
  {
    _logger.LogWarning("Sending 'DB Backup Missing' mail for: {name}", rule.Name);
    var smtpClient = _mailUtilsFactory.CreateSmtpClient();

    await smtpClient.SendMailAsync(new MailMessageBuilder()
      .WithTo(config.MailConfig.ToAddress, config.MailConfig.ToName)
      .WithSubject($"[DB Backup Missing] {rule.Name}")
      .WithFrom(_mailConfig)
      .WithHtmlBody(_mailTemplateHelper
        .GetTemplateBuilder("mariadb-file-missing")
        .AddPlaceHolder("file.path", rule.FilePath)
        .AddPlaceHolder("rule.name", rule.Name)
        .Process())
      .Build());
  }

  private async Task SendBackupFileSizeTooSmallEmail(DbBackupVerifyConfig config, DbBackupVerifyConfig.CheckRule rule, FileInfo fi)
  {
    _logger.LogWarning("Sending 'DB Backup Too Small' mail for: {name}", rule.Name);
    var smtpClient = _mailUtilsFactory.CreateSmtpClient();

    await smtpClient.SendMailAsync(new MailMessageBuilder()
      .WithTo(config.MailConfig.ToAddress, config.MailConfig.ToName)
      .WithSubject($"[DB Backup Too Small] {rule.Name}")
      .WithFrom(_mailConfig)
      .WithHtmlBody(_mailTemplateHelper
        .GetTemplateBuilder("mariadb-file-too-small")
        .AddPlaceHolder("file.path", rule.FilePath)
        .AddPlaceHolder("rule.name", rule.Name)
        .AddPlaceHolder("size.threshold", rule.MinFileSizeBytes)
        .AddPlaceHolder("size.actual", fi.Length)
        .Process())
      .Build());
  }

  private async Task SendCheckCompleteEmail(DbBackupVerifyConfig config)
  {
    _logger.LogInformation("Sending check completed email");
    var smtpClient = _mailUtilsFactory.CreateSmtpClient();

    await smtpClient.SendMailAsync(new MailMessageBuilder()
      .WithTo(config.MailConfig.ToAddress, config.MailConfig.ToName)
      .WithSubject("[DB Backup Check] Completed")
      .WithFrom(_mailConfig)
      .WithHtmlBody(_mailTemplateHelper
        .GetTemplateBuilder("mariadb-completed")
        .AddPlaceHolder("rule.count", config.Rules.Length)
        .Process())
      .Build());
  }

  private void ScheduleNextRunTime(RunningJobOptions options, VerifyMariaDbBackupsConfig config)
  {
    var tomorrow = DateTime.Now.AddDays(1);
    var nextRunTime = DateTimeOffset.Parse(config.NextRunTemplate
      .Replace("yyyy", tomorrow.Year.ToString())
      .Replace("MM", tomorrow.Month.ToString().PadLeft(2, '0'))
      .Replace("dd", tomorrow.Day.ToString().PadLeft(2, '0')));

    options.State.SetValue("NextRunTime", nextRunTime);
    _logger.LogInformation("Scheduled next run time for: {time}", nextRunTime);
  }
}

class VerifyMariaDbBackupsConfig
{
  public string ConfigFile { get; set; } = string.Empty;

  public string NextRunTemplate { get; set; } = string.Empty;

  public bool IsValid()
  {
    if (string.IsNullOrWhiteSpace(ConfigFile))
      return false;

    if (string.IsNullOrWhiteSpace(NextRunTemplate))
      return false;

    return true;
  }
}

class DbBackupVerifyConfig
{
  [JsonProperty("Mail")]
  public MailSettings MailConfig { get; set; } = new MailSettings();

  [JsonProperty("Rules")]
  public CheckRule[] Rules { get; set; } = Array.Empty<CheckRule>();

  public class CheckRule
  {
    [JsonProperty("Path")]
    public string FilePath { get; set; } = string.Empty;

    [JsonProperty("MinSizeKB")]
    public int MinFileSizeKb { get; set; }

    [JsonIgnore]
    public long MinFileSizeBytes { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; } = string.Empty;
  }

  public class MailSettings
  {
    [JsonProperty("ToAddress")]
    public string ToAddress { get; set; } = string.Empty;

    [JsonProperty("ToName")]
    public string ToName { get; set; } = string.Empty;
  }
}
