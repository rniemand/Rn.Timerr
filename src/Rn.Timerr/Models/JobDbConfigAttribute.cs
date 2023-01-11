using System.Text.RegularExpressions;

namespace Rn.Timerr.Models;

enum JobDbConfigType
{
  String,
  StringArray,
  Int,
  Bool,
  Regex
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
class JobDbConfigAttribute : Attribute
{
  public string PropertyName { get; set; }
  public JobDbConfigType ConfigType { get; set; } = JobDbConfigType.String;
  public bool ThrowIfMissing { get; set; }
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
