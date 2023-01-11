using Rn.Timerr.Models;

namespace Rn.Timerr.Jobs;

interface IRunnableJob
{
  string Name { get; }
  string ConfigKey { get; }

  Task<RunningJobResult> RunAsync(RunningJobOptions options);
}
