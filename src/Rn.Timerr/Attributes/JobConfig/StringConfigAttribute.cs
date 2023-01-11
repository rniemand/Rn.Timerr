using System.Reflection;
using Rn.Timerr.Enums;
using Rn.Timerr.Models;

namespace Rn.Timerr.Attributes;

class StringConfigAttribute : JobDbConfigAttribute
{
  public StringConfigAttribute(string propertyName)
    : base(propertyName, JobDbConfigType.String)
  { }

  public override void SetValue(PropertyInfo propertyInfo, object? obj, RunningJobOptions options)
  {
    propertyInfo.SetValue(obj, options.Config.GetStringValue(PropertyName));
  }
}
