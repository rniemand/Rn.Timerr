using NSubstitute;
using Rn.Timerr.Jobs;
using Rn.Timerr.Tests.TestSupport.Builders;
using RnCore.Abstractions;
using RnCore.Logging;
using RnCore.Mailer.Config;
using RnCore.Mailer.Factories;

namespace Rn.Timerr.Tests.Jobs.VerifyMariaDbBackupsTests;

static class TestHelper
{
  public static VerifyMariaDbBackups GetVerifyMariaDbBackups(ILoggerAdapter<VerifyMariaDbBackups>? logger = null,
    IMailTemplateHelper? mailTemplateHelper = null,
    RnMailConfig? mailConfig = null,
    IRnMailUtilsFactory? mailUtilsFactory = null,
    IFileAbstraction? fileAbstraction = null,
    IJsonHelper? jsonHelper = null)
  {
    return new VerifyMariaDbBackups(
      logger ?? Substitute.For<ILoggerAdapter<VerifyMariaDbBackups>>(),
      mailTemplateHelper ?? Substitute.For<IMailTemplateHelper>(),
      mailConfig ?? new RnMailConfigBuilder().Build(),
      mailUtilsFactory ?? Substitute.For<IRnMailUtilsFactory>(),
      fileAbstraction ?? Substitute.For<IFileAbstraction>(),
      jsonHelper ?? Substitute.For<IJsonHelper>());
  }
}
