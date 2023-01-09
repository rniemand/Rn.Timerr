using System.Runtime.Serialization;

namespace Rn.Timerr.Exceptions;

[Serializable]
public class RnTimerrException : Exception
{
  public RnTimerrException(string message)
  : base(message)
  { }

  protected RnTimerrException(SerializationInfo info, StreamingContext context)
    : base(info, context)
  {
    if (info == null)
      throw new ArgumentNullException(nameof(info));
  }
}
