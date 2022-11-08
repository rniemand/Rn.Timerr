using Rn.Timerr.Models;

namespace Rn.Timerr.Jobs;

class BackupSatisfactory : IRunnableJob
{
  public string Name => nameof(BackupSatisfactory);
  public bool CanRun(DateTime currentTime)
  {
    return true;
  }

  public async Task<JobOutcome> RunAsync(JobConfiguration jobConfig)
  {
    await Task.CompletedTask;

    return new JobOutcome();
  }
}
