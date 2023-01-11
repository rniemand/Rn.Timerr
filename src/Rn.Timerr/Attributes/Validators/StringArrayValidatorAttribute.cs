using Rn.Timerr.Enums;
using Rn.Timerr.Utils;

namespace Rn.Timerr.Attributes;

class StringArrayValidatorAttribute : ConfigValidatorAttribute
{
  public int MinLength
  {
    get => _minLength;
    set
    {
      _minLengthSet = true;
      _minLength = value;
    }
  }
  private bool _minLengthSet;
  private int _minLength;

  public StringArrayValidatorAttribute()
    : base(ConfigValidator.StringArray)
  { }

  public StringArrayValidatorAttribute(int minLength)
    : this()
  {
    MinLength = minLength;
  }


  public override bool Validate(string propName, object? rawValue, ValidationOutcome outcome)
  {
    if (rawValue is null)
    {
      outcome.WithError($"'{propName}' is required and cannot be NULL");
      return false;
    }

    if (rawValue is not string[] strArray)
    {
      var propType = rawValue.GetType().Name;
      outcome.WithError($"'{propName}' is of type '{propType}' and cannot be validated as a STRING[]");
      return false;
    }

    if (_minLengthSet && strArray.Length < MinLength)
    {
      outcome.WithError($"'{propName}' needs at lease {MinLength} value(s) - has {strArray.Length}");
      return false;
    }

    return true;
  }
}
