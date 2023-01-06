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
  private readonly RnMailConfig _mailConfig;

  public VerifyMariaDbBackups(ILoggerAdapter<VerifyMariaDbBackups> logger,
    IMailTemplateHelper mailTemplateHelper,
    RnMailConfig mailConfig)
  {
    _logger = logger;
    _mailTemplateHelper = mailTemplateHelper;
    _mailConfig = mailConfig;
  }


  // Interface methods
  public bool CanRun(RunningJobOptions options)
  {
    // TODO: [COMPLETE] (VerifyMariaDbBackups.CanRun) Complete this
    return true;
  }

  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
  {
    var outcome = new RunningJobResult(JobOutcome.Failed);

    var config = MapConfig(options);
    if (!config.IsValid())
      return outcome.WithError("Missing required configuration");

    var rules = GetCheckRules(config);
    if(rules.Count == 0)
      return outcome.WithError($"Unable to find config file: {config.ConfigFile}");

    foreach (var rule in rules)
    {
      if (!File.Exists(rule.FilePath))
      {
        await SendFileNotFoundEmail(config, rule);
        continue;
      }



      Console.WriteLine();
      Console.WriteLine();
    }


    await Task.CompletedTask;



    Console.WriteLine();
    return outcome.AsSucceeded();
  }


  // Internal methods
  private VerifyMariaDbBackupsConfig MapConfig(RunningJobOptions options) => new()
  {
    ConfigFile = options.Config.GetStringValue("configFile")
  };

  private List<DbBackupVerifyConfig> GetCheckRules(VerifyMariaDbBackupsConfig config)
  {
    // TODO: [ABSTRACT] (VerifyMariaDbBackups.GetCheckRules) Use abstraction for this
    if (!File.Exists(config.ConfigFile))
      return new List<DbBackupVerifyConfig>();

    var parser = new TemplateStringParser();
    var rawJson = File.ReadAllText(config.ConfigFile);
    var rules = JsonConvert.DeserializeObject<List<DbBackupVerifyConfig>>(rawJson);

    foreach (var rule in rules)
    {
      rule.FilePath = parser.Parse(rule.FilePath);
    }

    return rules;
  }

  private async Task SendFileNotFoundEmail(VerifyMariaDbBackupsConfig config, DbBackupVerifyConfig rule)
  {
    var mailMessage = new MailMessageBuilder()
      .WithTo("niemand.richard@gmail.com")
      .WithSubject("Testing email code")
      .WithFrom(_mailConfig)
      .WithHtmlBody(_mailTemplateHelper
        .GetTemplateBuilder("mariadb-file-missing")
        .AddPlaceHolder("file.path", rule.FilePath)
        .AddPlaceHolder("rule.name", rule.Name)
        .Process())
      .Build();

    var mailMessageBody = mailMessage.Body;


    Console.WriteLine();
    Console.WriteLine();
  }
}

class VerifyMariaDbBackupsConfig
{
  public string ConfigFile { get; set; } = string.Empty;

  public bool IsValid()
  {
    if (string.IsNullOrWhiteSpace(ConfigFile))
      return false;

    return true;
  }
}

class DbBackupVerifyConfig
{
  [JsonProperty("Path")]
  public string FilePath { get; set; } = string.Empty;

  [JsonProperty("MinSizeKB")]
  public int MinFileSizeKb { get; set; } = 0;

  [JsonProperty("Name")]
  public string Name { get; set; } = string.Empty;
}
