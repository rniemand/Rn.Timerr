namespace Rn.Timerr.Models.Entities;

public class ConfigEntity
{
  public string Category { get; set; } = string.Empty;
  public string Key { get; set; } = string.Empty;
  public bool Collection { get; set; }
  public string Host { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public string Value { get; set; } = string.Empty;
}
