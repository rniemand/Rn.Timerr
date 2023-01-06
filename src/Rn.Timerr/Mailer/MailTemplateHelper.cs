using RnCore.Logging;

namespace Rn.Timerr.Mailer;

public interface IMailTemplateHelper
{
  MailTemplateBuilder GetTemplateBuilder(string templateName);
}

public class MailTemplateHelper : IMailTemplateHelper
{
  private readonly ILoggerAdapter<MailTemplateHelper> _logger;
  private readonly IMailTemplateProvider _tplProvider;
  private readonly RnMailConfig _mailConfig;

  public MailTemplateHelper(
    ILoggerAdapter<MailTemplateHelper> logger,
    IMailTemplateProvider tplProvider,
    RnMailConfig mailConfig)
  {
    _logger = logger;
    _tplProvider = tplProvider;
    _mailConfig = mailConfig;
  }

  public MailTemplateBuilder GetTemplateBuilder(string templateName)
  {
    _logger.LogDebug("Resolving template: {name}", templateName);
    var templateBuilder = new MailTemplateBuilder
    {
      RawTemplate = _tplProvider.GetTemplate(templateName),
      TemplateName = templateName
    };

    if (!templateBuilder.TemplateFound)
      return templateBuilder;

    ProcessCssTags(templateBuilder);
    InjectGlobalPlaceholders(templateBuilder);
    return templateBuilder;
  }

  private void ProcessCssTags(MailTemplateBuilder builder)
  {
    // (\{css:([^\}]+)\})
    const string regex = "(\\{css:([^\\}]+)\\})";
    if (!builder.RawTemplate.MatchesRegex(regex))
      return;

    var matches = builder.RawTemplate.GetRegexMatches(regex);
    foreach (var groups in matches.Select(x => x.Groups))
      builder.ReplaceCssTag(groups[1].Value, _tplProvider.GetCss(groups[2].Value));
  }

  private void InjectGlobalPlaceholders(MailTemplateBuilder builder)
  {
    if (_mailConfig.TemplatePlaceholders.Count == 0)
      return;

    foreach (var placeholder in _mailConfig.TemplatePlaceholders)
      builder.AddPlaceHolder($"global.{placeholder.Key}", placeholder.Value);
  }
}
