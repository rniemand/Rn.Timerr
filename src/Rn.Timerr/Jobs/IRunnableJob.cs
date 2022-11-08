using Rn.Timerr.Models;

namespace Rn.Timerr.Jobs;

interface IRunnableJob
{
  string Name { get; }

  bool CanRun(DateTime currentTime);

  Task<JobOutcome> RunAsync(JobConfiguration jobConfig);
}
