using Rn.Timerr.Attributes;
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
      .Where(p => p.CustomAttributes.Any(a => a.AttributeType.IsAssignableTo(attributeType)))
      .ToList();

    var objInstance = new TClass();
    foreach (var propertyInfo in propertyInfos)
    {
      if (Attribute.GetCustomAttribute(propertyInfo, attributeType) is not JobDbConfigAttribute attribute)
        continue;

      attribute.SetValue(propertyInfo, objInstance, options);
    }

    return objInstance;
  }

  public static ValidationOutcome ValidateConfig(object jobConfig)
  {
    var outcome = new ValidationOutcome();
    var attributeType = typeof(ConfigValidatorAttribute);

    var propertyInfos = jobConfig.GetType()
      .GetProperties()
      .Where(p => p.CustomAttributes.Any())
      .Where(p => p.CustomAttributes.Any(a => a.AttributeType.IsAssignableTo(attributeType)))
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
}
