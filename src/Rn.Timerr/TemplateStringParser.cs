using System.Text.RegularExpressions;

namespace Rn.Timerr;

class TemplateStringParser
{
  // {date:YYYY-mm}
  private static Regex RX_DATE = new("(\\{date:([^\\}]+)\\})", RegexOptions.Compiled);


  public string Parse(string template)
  {
    template = processDatePlaceholders(template);

    return template;
  }


  // Internal methods
  private string processDatePlaceholders(string template)
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
