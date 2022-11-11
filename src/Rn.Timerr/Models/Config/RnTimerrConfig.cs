using Microsoft.Extensions.Configuration;

namespace Rn.Timerr.Models.Config;

class RnTimerrConfig
{
  [ConfigurationKeyName("Host")]
  public string Host { get; set; } = string.Empty;
}
