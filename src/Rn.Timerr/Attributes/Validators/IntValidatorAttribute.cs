using Rn.Timerr.Enums;
using Rn.Timerr.Utils;

namespace Rn.Timerr.Attributes;

class IntValidatorAttribute : ConfigValidatorAttribute
{
  public int MinValue
  {
    get => _minValue;
    set
    {
      _minValue = value;
      _minValueSet = true;
    }
  }

  private bool _minValueSet;
  private int _minValue;

  public int MaxValue
  {
    get => _maxValue;
    set
    {
      _maxValueSet = true;
      _maxValue = value;
    }
  }

  private bool _maxValueSet;
  private int _maxValue;

  public IntValidatorAttribute()
    : base(ConfigValidator.Int)
  { }

  public IntValidatorAttribute(int min)
    : this()
  {
    MinValue = min;
  }

  public IntValidatorAttribute(int min, int max)
    : this(min)
  {
    MaxValue = max;
  }


  public override bool Validate(string propName, object? rawValue, ValidationOutcome outcome)
  {
    if (rawValue is null)
    {
      outcome.WithError($"'{propName}' is required and cannot be NULL");
      return false;
    }

    if (rawValue is not int intValue)
    {
      var propType = rawValue.GetType().Name;
      outcome.WithError($"'{propName}' is of type '{propType}' and cannot be validated as a INT");
      return false;
    }

    if (_minValueSet && intValue < MinValue)
    {
      outcome.WithError($"'{propName}' must be >= {MinValue}");
      return false;
    }

    if (_maxValueSet && intValue > MaxValue)
    {
      outcome.WithError($"'{propName}' must be <= {MaxValue}");
      return false;
    }

    return true;
  }
}
