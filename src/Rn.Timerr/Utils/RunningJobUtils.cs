using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Rn.Timerr.Attributes;
using Rn.Timerr.Enums;
using Rn.Timerr.Exceptions;
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
      HandleThrowIfMissing(attribute, @class, propertyInfo);
    }

    return @class;
  }

  public static ValidationOutcome ValidateConfig(object jobConfig)
  {
    var outcome = new ValidationOutcome();
    var attributeType = typeof(JobConfigValidatorAttribute);
    var propertyInfos = jobConfig.GetType()
      .GetProperties()
      .Where(p => p.CustomAttributes.Any())
      .Where(p => p.CustomAttributes.Any(a => a.AttributeType == attributeType))
      .ToList();

    foreach (var propertyInfo in propertyInfos)
    {
      if (Attribute.GetCustomAttribute(propertyInfo, attributeType) is not JobConfigValidatorAttribute attribute)
        continue;

      var rawValue = propertyInfo.GetValue(jobConfig);
      var propName = propertyInfo.Name;

      if (attribute.Validator == ConfigValidator.String)
      {
        if (RunStringValidator(attribute, propName, rawValue, outcome))
          continue;
        return outcome;
      }

      throw new ArgumentOutOfRangeException();
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

  private static bool RunStringValidator(JobConfigValidatorAttribute attribute, string propName, object? rawValue, ValidationOutcome outcome)
  {
    // Handle NULL values
    if (rawValue is null)
    {
      if (!attribute.Required) return true;
      outcome.WithError($"'{propName}' is required and cannot be NULL");
      return false;
    }

    // Handle values of the wrong type
    if (rawValue is not string strValue)
    {
      var propType = rawValue.GetType().Name;
      outcome.WithError($"'{propName}' is of type '{propType}' and cannot be validated as a STRING");
      return false;
    }

    // Validate a string value
    if (string.IsNullOrWhiteSpace(strValue))
    {
      if (!attribute.Required) return true;
      outcome.WithError($"'{propName}' is required and cannot be EMPTY or WHITE_SPACE");
      return false;
    }

    return true;
  }
}
