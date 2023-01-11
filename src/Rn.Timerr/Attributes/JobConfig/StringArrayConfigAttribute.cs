using System.Reflection;
using Rn.Timerr.Enums;
using Rn.Timerr.Models;

namespace Rn.Timerr.Attributes;

class StringArrayConfigAttribute : JobDbConfigAttribute
{
  public StringArrayConfigAttribute(string propertyName)
    : base(propertyName, JobDbConfigType.StringArray)
  { }

  public override void SetValue(PropertyInfo propertyInfo, object? obj, RunningJobOptions options)
  {
    propertyInfo.SetValue(obj, options.Config.GetStringCollection(PropertyName).ToArray());
  }
}
