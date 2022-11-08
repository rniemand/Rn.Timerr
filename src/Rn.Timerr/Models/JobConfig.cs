using Microsoft.Extensions.Configuration;

namespace Rn.Timerr.Models;

class JobConfig
{
  private readonly IConfiguration _configuration = new ConfigurationManager();

  public JobConfig() { }

  public JobConfig(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public bool HasStringValue(string key) => _configuration.GetValue<string>(key) is not null;
  
  public string GetStringValue(string key) => _configuration.GetValue<string>(key) ?? string.Empty;

  public bool GetBoolValue(string key, bool fallback) => _configuration.GetValue<bool>(key, fallback);
}
