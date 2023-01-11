using System.Reflection;
using System.Text.RegularExpressions;
using Rn.Timerr.Exceptions;
using Rn.Timerr.Models;

namespace Rn.Timerr.Utils;

static class RunningJobUtils
{
  public static TClass MapConfiguration<TClass>(RunningJobOptions options) where TClass : class, new()
  {
    var attributeType = typeof(JobDbConfigAttribute);
    var propertyInfos = typeof(TClass)
      .GetProperties()
      .Where(p => p.CustomAttributes.Any())
      .Where(p => p.CustomAttributes.Any(a => a.AttributeType == attributeType))
      .ToList();

    var @class = new TClass();

    foreach (var propertyInfo in propertyInfos)
    {
      if (Attribute.GetCustomAttribute(propertyInfo, attributeType) is not JobDbConfigAttribute attribute)
        continue;

      MapObjectValue(attribute, @class, propertyInfo, options);
      HandleThrowIfMissing(attribute, @class, propertyInfo);
    }

    return @class;
  }

  // Internal methods
  private static void MapObjectValue<TClass>(JobDbConfigAttribute attribute, TClass instance, PropertyInfo propertyInfo, RunningJobOptions options)
  {
    switch (attribute.ConfigType)
    {
      case JobDbConfigType.String:
        propertyInfo.SetValue(instance, options.Config.GetStringValue(attribute.PropertyName));
        return;

      case JobDbConfigType.StringArray:
        propertyInfo.SetValue(instance, options.Config.GetStringCollection(attribute.PropertyName));
        return;

      case JobDbConfigType.Int:
        propertyInfo.SetValue(instance, options.Config.GetIntValue(attribute.PropertyName, attribute.IntFallback));
        return;

      case JobDbConfigType.Bool:
        propertyInfo.SetValue(instance, options.Config.GetBoolValue(attribute.PropertyName, attribute.BoolFallback));
        return;

      case JobDbConfigType.Regex:
        var rxString = options.Config.GetStringValue(attribute.PropertyName);
        if (!string.IsNullOrWhiteSpace(rxString))
        {
          propertyInfo.SetValue(instance, new Regex(rxString, attribute.RegexOptions));
        }
        return;

      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private static void HandleThrowIfMissing<TClass>(JobDbConfigAttribute attribute, TClass instance, PropertyInfo propertyInfo)
  {
    if (!attribute.ThrowIfMissing)
      return;

    var instanceValue = propertyInfo.GetValue(instance);

    if (attribute.ConfigType == JobDbConfigType.String)
    {
      if (string.IsNullOrWhiteSpace(instanceValue as string))
        throw new RnTimerrException($"Missing configuration for: {attribute.PropertyName}");
      return;
    }

    throw new ArgumentOutOfRangeException();
  }
}
