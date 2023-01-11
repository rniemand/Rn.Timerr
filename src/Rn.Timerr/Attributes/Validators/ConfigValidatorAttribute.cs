using Rn.Timerr.Enums;
using Rn.Timerr.Utils;

namespace Rn.Timerr.Attributes;

[AttributeUsage(AttributeTargets.Property)]
abstract class ConfigValidatorAttribute : Attribute
{
  public ConfigValidator Validator { get; }

  protected ConfigValidatorAttribute(ConfigValidator validator)
  {
    Validator = validator;
  }

  public virtual bool Validate(string propName, object? rawValue, ValidationOutcome outcome)
  {
    throw new NotImplementedException();
  }
}
