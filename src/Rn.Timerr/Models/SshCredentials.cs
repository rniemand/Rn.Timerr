using Newtonsoft.Json;

namespace Rn.Timerr.Models;

class SshCredentials
{
  [JsonProperty("host")]
  public string Host { get; set; } = string.Empty;

  [JsonProperty("port")]
  public int Port { get; set; } = 22;

  [JsonProperty("user")]
  public string User { get; set; } = string.Empty;

  [JsonProperty("pass")]
  public string Pass { get; set; } = string.Empty;


  public bool IsValid()
  {
    if (string.IsNullOrWhiteSpace(Host))
      return false;

    if (string.IsNullOrWhiteSpace(User))
      return false;

    if (string.IsNullOrWhiteSpace(Pass))
      return false;

    return true;
  }
}
