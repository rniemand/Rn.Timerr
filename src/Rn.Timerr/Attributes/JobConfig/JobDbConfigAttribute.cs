using System.Reflection;
using Rn.Timerr.Enums;
using Rn.Timerr.Models;

namespace Rn.Timerr.Attributes;

[AttributeUsage(AttributeTargets.Property)]
abstract class JobDbConfigAttribute : Attribute
{
  public string PropertyName { get; }
  public JobDbConfigType ConfigType { get; }

  protected JobDbConfigAttribute(string propertyName, JobDbConfigType type)
  {
    PropertyName = propertyName;
    ConfigType = type;
  }

  public virtual void SetValue(PropertyInfo propertyInfo, object? obj, RunningJobOptions options)
  {
    throw new NotImplementedException();
  }
}
