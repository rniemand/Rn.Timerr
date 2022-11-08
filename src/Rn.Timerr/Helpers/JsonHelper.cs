using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Rn.Timerr.Helpers;

// TODO: Break out into own lib
public interface IJsonHelper
{
  T DeserializeObject<T>(string value);
  object? DeserializeObject(string value, Type type);
  bool TryDeserializeObject<T>(string json, out T parsed);
  string SerializeObject(object value);
  string SerializeObject(object value, bool formatted);
}

public class JsonHelper : IJsonHelper
{
  [ExcludeFromCodeCoverage]
  public T DeserializeObject<T>(string value) =>
    JsonConvert.DeserializeObject<T>(value);

  [ExcludeFromCodeCoverage]
  public object? DeserializeObject(string value, Type type) =>
    JsonConvert.DeserializeObject(value, type);

  public bool TryDeserializeObject<T>(string json, out T parsed)
  {
    parsed = default;

    try
    {
      parsed = JsonConvert.DeserializeObject<T>(json);
      return true;
    }
    catch (Exception)
    {
      return false;
    }
  }

  [ExcludeFromCodeCoverage]
  public string SerializeObject(object value) =>
    SerializeObject(value, false);

  [ExcludeFromCodeCoverage]
  public string SerializeObject(object value, bool formatted) =>
    JsonConvert.SerializeObject(value, formatted ? Formatting.Indented : Formatting.None);
}
