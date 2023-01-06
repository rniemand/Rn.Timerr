using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Rn.Timerr.Mailer;

[ExcludeFromCodeCoverage]
public class MailTemplateBuilder
{
  public bool TemplateFound => !string.IsNullOrWhiteSpace(RawTemplate);
  public string RawTemplate { get; set; } = string.Empty;
  public string TemplateName { get; set; } = string.Empty;
  public Dictionary<string, object> Placeholders { get; set; } = new();

  public MailTemplateBuilder AddPlaceHolder(string key, object value)
  {
    Placeholders[key] = value;
    return this;
  }

  public MailTemplateBuilder AddPlaceholders(Dictionary<string, object> placeholders)
  {
    foreach (var placeholder in placeholders)
    {
      Placeholders[placeholder.Key] = placeholder.Value;
    }

    return this;
  }

  public MailTemplateBuilder ReplaceCssTag(string placeholder, string css)
  {
    RawTemplate = RawTemplate.Replace(placeholder, $"<style>{css}</style>");
    return this;
  }

  public string Process()
  {
    var processed = RawTemplate;

    const string regex = "(\\{\\{([^\\}]+)\\}\\})";
    if (!processed.MatchesRegex(regex))
      return processed;

    var matches = processed.GetRegexMatches(regex);
    foreach (Match match in matches)
    {
      var placeholder = match.Groups[1].Value;
      processed = processed.Replace(placeholder, ResolvePlaceholder(placeholder));
    }

    return processed;
  }

  private string ResolvePlaceholder(string placeholder)
  {
    placeholder = placeholder
      .Replace("{", "")
      .Replace("}", "");

    if (!placeholder.Contains(':'))
      return GetStringPlaceholder(placeholder, string.Empty);

    var parts = placeholder.Split(":");
    var key = parts[0];
    var format = parts[1].Replace("'", "");

    return GetStringPlaceholder(key, format);
  }

  private string GetStringPlaceholder(string key, string args)
  {
    if (!Placeholders.ContainsKey(key))
      return string.Empty;

    var rawValue = Placeholders[key];

    if (rawValue is string strPlaceholder)
      return strPlaceholder;

    if (rawValue is int intValue)
      return intValue.ToString(string.IsNullOrWhiteSpace(args) ? "D" : args);

    if (rawValue is long longValue)
      return longValue.ToString(string.IsNullOrWhiteSpace(args) ? "D" : args);

    if (rawValue is bool boolValue)
      return boolValue ? "true" : "false";

    if (rawValue is DateTime dateValue)
      return dateValue.ToString(string.IsNullOrWhiteSpace(args) ? "s" : args);

    if (rawValue is float floatValue)
      return floatValue.ToString(string.IsNullOrWhiteSpace(args) ? "G" : args);

    if (rawValue is double doubleValue)
      return doubleValue.ToString(string.IsNullOrWhiteSpace(args) ? "G" : args);

    var valueType = rawValue.GetType().Name;
    return $"(UNSUPPORTED:{valueType})";
  }
}
