using System.Reflection;
using Rn.Timerr.Enums;
using Rn.Timerr.Models;

namespace Rn.Timerr.Attributes;

class IntConfigAttribute : JobDbConfigAttribute
{
  public int Fallback
  {
    get => _fallback;
    set
    {
      _fallback = value;
      _fallbackSet = true;
    }
  }
  private bool _fallbackSet;
  private int _fallback;

  public IntConfigAttribute(string propertyName)
    : base(propertyName, JobDbConfigType.Int)
  { }

  public IntConfigAttribute(string propertyName, int fallback)
    : this(propertyName)
  {
    Fallback = fallback;
  }

  public override void SetValue(PropertyInfo propertyInfo, object? obj, RunningJobOptions options)
  {
    var fallback = _fallbackSet ? _fallback : 0;
    propertyInfo.SetValue(obj, options.Config.GetIntValue(PropertyName, fallback));
  }
}
