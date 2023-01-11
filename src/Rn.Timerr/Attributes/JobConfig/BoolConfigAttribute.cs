using System.Reflection;
using Rn.Timerr.Enums;
using Rn.Timerr.Models;

namespace Rn.Timerr.Attributes;

class BoolConfigAttribute : JobDbConfigAttribute
{
  public bool Fallback { get; set; }

  public BoolConfigAttribute(string propertyName)
    : base(propertyName, JobDbConfigType.Bool)
  { }

  public BoolConfigAttribute(string propertyName, bool fallback)
    : this(propertyName)
  {
    Fallback = fallback;
  }

  public override void SetValue(PropertyInfo propertyInfo, object? obj, RunningJobOptions options)
  {
    propertyInfo.SetValue(obj, options.Config.GetBoolValue(PropertyName, Fallback));
  }
}
