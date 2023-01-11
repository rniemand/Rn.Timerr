using Rn.Timerr.Enums;

namespace Rn.Timerr.Attributes;

[AttributeUsage(AttributeTargets.Property)]
class JobConfigValidatorAttribute : Attribute
{
  public ConfigValidator Validator { get; set; }
  public bool Required { get; set; }

  public JobConfigValidatorAttribute(ConfigValidator validator)
  {
    Validator = validator;
  }

  public JobConfigValidatorAttribute(ConfigValidator validator, bool required)
    : this(validator)
  {
    Required = required;
  }
}
