using System.Reflection;
using System.Text.RegularExpressions;
using Rn.Timerr.Attributes;
using Rn.Timerr.Enums;
using Rn.Timerr.Models;

namespace Rn.Timerr.Utils;

class ValidationOutcome
{
  public bool Success { get; set; } = true;
  public string ValidationError { get; set; } = string.Empty;

  public ValidationOutcome WithError(string error)
  {
    Success = false;
    ValidationError = error;
    return this;
  }
}

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
    }

    return @class;
  }

  public static ValidationOutcome ValidateConfig(object jobConfig)
  {
    var outcome = new ValidationOutcome();
    var attributeType = typeof(ConfigValidatorAttribute);

    var propertyInfos = jobConfig.GetType()
      .GetProperties()
      .Where(p => p.CustomAttributes.Any())
      .Where(p => p.CustomAttributes.Any(a => a.AttributeType == attributeType))
      .ToList();

    foreach (var propertyInfo in propertyInfos)
    {
      if (Attribute.GetCustomAttribute(propertyInfo, attributeType) is not ConfigValidatorAttribute attribute)
        continue;

      if (!attribute.Validate(propertyInfo.Name, propertyInfo.GetValue(jobConfig), outcome))
        return outcome;
    }

    return outcome;
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
        propertyInfo.SetValue(instance, options.Config.GetStringCollection(attribute.PropertyName).ToArray());
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
}
