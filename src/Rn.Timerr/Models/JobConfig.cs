using Rn.Timerr.Enums;
using Rn.Timerr.Models.Entities;

namespace Rn.Timerr.Models;

public class JobConfig
{
  private readonly Dictionary<string, ConfigEntity> _config = new(StringComparer.InvariantCultureIgnoreCase);
  private readonly Dictionary<string, List<ConfigEntity>> _collectionConfig = new(StringComparer.InvariantCultureIgnoreCase);

  public JobConfig() { }

  public JobConfig(IReadOnlyCollection<ConfigEntity> config)
    : this()
  {
    foreach (var currentConfig in config.Where(c => c.Host == "*"))
    {
      if (currentConfig.Collection)
      {
        if (!_collectionConfig.ContainsKey(currentConfig.Key))
          _collectionConfig[currentConfig.Key] = new List<ConfigEntity>();
        _collectionConfig[currentConfig.Key].Add(currentConfig);
      }
      else
      {
        _config[currentConfig.Key] = currentConfig;
      }
    }

    foreach (var currentConfig in config.Where(c => c.Host != "*"))
    {
      if (currentConfig.Collection)
      {
        if (!_collectionConfig.ContainsKey(currentConfig.Key))
          _collectionConfig[currentConfig.Key] = new List<ConfigEntity>();
        _collectionConfig[currentConfig.Key].Add(currentConfig);
      }
      else
      {
        _config[currentConfig.Key] = currentConfig;
      }
    }
  }

  public bool HasStringValue(string key)
  {
    if (!_config.ContainsKey(key))
      return false;

    return _config[key].Type.ToLower() == DbValueType.String;
  }

  public bool HasDateTimeValue(string key)
  {
    if (!_config.ContainsKey(key))
      return false;

    return _config[key].Type.ToLower() == DbValueType.DateTime;
  }

  public bool HasIntValue(string key)
  {
    if (!_config.ContainsKey(key))
      return false;

    return _config[key].Type.ToLower() == DbValueType.Int;
  }

  public bool HasBoolValue(string key)
  {
    if (!_config.ContainsKey(key))
      return false;

    return _config[key].Type.ToLower() == DbValueType.Boolean;
  }

  public string GetStringValue(string key) => !HasStringValue(key) ? string.Empty : _config[key].Value;

  public List<string> GetStringCollection(string key) => !_collectionConfig.ContainsKey(key)
    ? new List<string>()
    : _collectionConfig[key].Select(entry => entry.Value).ToList();

  public int GetIntValue(string key, int fallback)
  {
    if (!HasIntValue(key))
      return fallback;

    return int.TryParse(_config[key].Value, out var parsed) ? parsed : fallback;
  }

  public bool GetBoolValue(string key, bool fallback)
  {
    if (!HasBoolValue(key))
      return fallback;

    return bool.TryParse(_config[key].Value, out var parsed) ? parsed : fallback;
  }

  public DateTimeOffset GetDateTimeOffsetValue(string key, DateTimeOffset fallback) =>
    !HasDateTimeValue(key) ? fallback : DateTimeOffset.Parse(_config[key].Value);

  public int GetOptionCount() => _config.Count;
}
