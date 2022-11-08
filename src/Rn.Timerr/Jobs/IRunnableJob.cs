using Rn.Timerr.Models;

namespace Rn.Timerr.Jobs;

interface IRunnableJob
{
  string Name { get; }
  string ConfigKey { get; }

  bool CanRun(JobOptions jobOptions);

  Task<JobOutcome> RunAsync(JobOptions jobOptions);
}
