using Rn.Timerr.Models;

namespace Rn.Timerr.Tests.TestSupport.Builders;

public class RunningJobOptionsBuilder
{
  private string _configKey = string.Empty;
  private string _host = string.Empty;

  public RunningJobOptionsBuilder WithDefaults() => WithConfigKey("CustomJob")
    .WithHost("TestHost");

  public RunningJobOptionsBuilder WithConfigKey(string configKey)
  {
    _configKey = configKey;
    return this;
  }

  public RunningJobOptionsBuilder WithHost(string host)
  {
    _host = host;
    return this;
  }

  public RunningJobOptions Build() => new(_configKey, _host);
}
