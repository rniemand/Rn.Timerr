namespace Rn.Timerr.Extensions;

static class StringExtensions
{
  public static bool IgnoreCaseEquals(this string value, string compare) =>
    value.Equals(compare, StringComparison.InvariantCultureIgnoreCase);
}
