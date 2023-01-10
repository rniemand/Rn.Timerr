using Rn.Timerr.Exceptions;

namespace Rn.Timerr.Extensions;

public static class JobStateExtensions
{
  public static bool HasStateKey(this Dictionary<string, object> state, string key) => state.ContainsKey(key);

  public static DateTime GetDateTimeValue(this Dictionary<string, object> state, string key)
  {
    if (!HasStateKey(state, key))
      throw new RnTimerrException($"Unable to find key: {key}");

    return (DateTime)state[key];
  }
}
