using RnCore.Mailer.Config;

namespace Rn.Timerr.Tests.TestSupport.Builders;

public class RnMailConfigBuilder
{
  private readonly RnMailConfig _config = new RnMailConfig();

  public RnMailConfig Build() => _config;
}
