using Rn.Timerr.Enums;
using Rn.Timerr.Utils;

namespace Rn.Timerr.Attributes;

class StringValidatorAttribute : ConfigValidatorAttribute
{
  public StringValidatorAttribute()
    : base(ConfigValidator.String)
  { }

  public override bool Validate(string propName, object? rawValue, ValidationOutcome outcome)
  {
    if (rawValue is null)
    {
      outcome.WithError($"'{propName}' is required and cannot be NULL");
      return false;
    }

    if (rawValue is not string strValue)
    {
      var propType = rawValue.GetType().Name;
      outcome.WithError($"'{propName}' is of type '{propType}' and cannot be validated as a STRING");
      return false;
    }

    if (!string.IsNullOrWhiteSpace(strValue))
      return true;

    outcome.WithError($"'{propName}' is required and cannot be EMPTY or WHITE_SPACE");
    return false;
  }
}
