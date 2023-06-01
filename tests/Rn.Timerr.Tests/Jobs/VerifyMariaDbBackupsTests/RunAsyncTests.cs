using Rn.Timerr.Enums;
using Rn.Timerr.Tests.TestSupport.Builders;

namespace Rn.Timerr.Tests.Jobs.VerifyMariaDbBackupsTests;

[TestFixture]
class RunAsyncTests
{
  [Test]
  public async Task RunAsync_GivenInvalidConfig_ShouldReturnJobFailure()
  {
    // arrange
    var job = TestHelper.GetVerifyMariaDbBackups();

    // act
    var jobResult = await job.RunAsync(new RunningJobOptionsBuilder().WithDefaults().Build());

    // assert
    Assert.That(jobResult.Outcome, Is.EqualTo(JobOutcome.Failed));
  }
}
