using System.Text.RegularExpressions;

namespace Rn.Timerr;

class TemplateStringParser
{
  private static Regex RX_DATE = new("(\\{date:([^\\}]+)\\})", RegexOptions.Compiled);

  protected TemplateStringParser() { }

  public static string Parse(string template)
  {
    template = ProcessDatePlaceholders(template);

    return template;
  }


  // Internal methods
  private static string ProcessDatePlaceholders(string template)
  {
    if (!RX_DATE.IsMatch(template))
      return template;

    var now = DateTime.Now;

    do
    {
      var match = RX_DATE.Match(template);
      template = template.Replace(match.Groups[1].Value, now.ToString(match.Groups[2].Value));

    } while (RX_DATE.IsMatch(template));

    return template;
  }
}
