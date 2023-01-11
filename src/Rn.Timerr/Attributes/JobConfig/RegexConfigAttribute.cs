using System.Reflection;
using Rn.Timerr.Enums;
using System.Text.RegularExpressions;
using Rn.Timerr.Models;

namespace Rn.Timerr.Attributes;

class RegexConfigAttribute : JobDbConfigAttribute
{
  public RegexOptions RegexOptions { get; set; } = RegexOptions.None;

  public RegexConfigAttribute(string propertyName)
    : base(propertyName, JobDbConfigType.Regex)
  { }

  public RegexConfigAttribute(string propertyName, RegexOptions options)
    : this(propertyName)
  {
    RegexOptions = options;
  }

  public override void SetValue(PropertyInfo propertyInfo, object? obj, RunningJobOptions options)
  {
    var rxString = options.Config.GetStringValue(PropertyName);

    if (string.IsNullOrWhiteSpace(rxString))
      return;

    propertyInfo.SetValue(obj, new Regex(rxString, RegexOptions));
  }
}
