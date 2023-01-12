using Newtonsoft.Json;
using Rn.Timerr.Attributes;
using Rn.Timerr.Enums;
using Rn.Timerr.Models;
using Rn.Timerr.Utils;
using RnCore.Abstractions;
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
  private readonly IFileAbstraction _file;
  private readonly IJsonHelper _jsonHelper;
  private readonly RnMailConfig _mailConfig;

  public VerifyMariaDbBackups(ILoggerAdapter<VerifyMariaDbBackups> logger,
    IMailTemplateHelper mailTemplateHelper,
    RnMailConfig mailConfig,
    IRnMailUtilsFactory mailUtilsFactory,
    IFileAbstraction file,
    IJsonHelper jsonHelper)
  {
    _logger = logger;
    _mailTemplateHelper = mailTemplateHelper;
    _mailConfig = mailConfig;
    _mailUtilsFactory = mailUtilsFactory;
    _file = file;
    _jsonHelper = jsonHelper;
  }


  // Interface methods
  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
  {
    var outcome = new RunningJobResult(JobOutcome.Failed);

    // Map and validate the jobs configuration
    var config = RunningJobUtils.MapConfiguration<Config>(options);
    var validationOutcome = RunningJobUtils.ValidateConfig(config);
    if (!validationOutcome.Success)
      return outcome.WithError(validationOutcome.ValidationError);

    var checkConfig = GetDbCheckConfig(config);
    if (checkConfig.Rules.Length == 0)
      return outcome.WithError($"Unable to find config file: {config.ConfigFile}");

    foreach (var rule in checkConfig.Rules)
    {
      // Ensure that the backup file exists
      if (!_file.Exists(rule.FilePath))
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

    options.ScheduleNextRunUsingTemplate(DateTime.Now.AddDays(1), config.NextRunTemplate);
    return outcome.AsSucceeded();
  }


  // Internal methods
  private JsonConfig GetDbCheckConfig(Config config)
  {
    if (!_file.Exists(config.ConfigFile))
      return new JsonConfig();

    var rawJson = _file.ReadAllText(config.ConfigFile);
    var parsedConfig = _jsonHelper.DeserializeObject<JsonConfig>(rawJson);

    foreach (var rule in parsedConfig!.Rules)
    {
      rule.FilePath = TemplateStringParser.Parse(rule.FilePath);
      rule.MinFileSizeBytes = rule.MinFileSizeKb * 1024;
    }

    return parsedConfig;
  }

  private async Task SendFileNotFoundEmail(JsonConfig config, JsonConfig.CheckRule rule)
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

  private async Task SendBackupFileSizeTooSmallEmail(JsonConfig config, JsonConfig.CheckRule rule, FileInfo fi)
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

  private async Task SendCheckCompleteEmail(JsonConfig config)
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


  // Supporting classes
  class Config
  {
    [StringConfig("configFile")]
    [StringValidator]
    public string ConfigFile { get; set; } = string.Empty;

    [StringConfig("NextRunTemplate")]
    [StringValidator]
    public string NextRunTemplate { get; set; } = string.Empty;
  }

  class JsonConfig
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
}
