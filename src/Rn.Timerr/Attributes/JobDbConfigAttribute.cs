using System.Text.RegularExpressions;
using Rn.Timerr.Enums;

namespace Rn.Timerr.Attributes;

[AttributeUsage(AttributeTargets.Property)]
class JobDbConfigAttribute : Attribute
{
  public string PropertyName { get; set; }
  public JobDbConfigType ConfigType { get; set; } = JobDbConfigType.String;
  public int IntFallback { get; set; }
  public bool BoolFallback { get; set; }
  public RegexOptions RegexOptions { get; set; } = RegexOptions.None;

  public JobDbConfigAttribute(string propertyName)
  {
    PropertyName = propertyName;
  }

  public JobDbConfigAttribute(string propertyName, JobDbConfigType type)
    : this(propertyName)
  {
    ConfigType = type;
  }
}
