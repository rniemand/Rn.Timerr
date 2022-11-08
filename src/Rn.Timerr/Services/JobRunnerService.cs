using Rn.Timerr.Jobs;
using Rn.Timerr.Models;
using RnCore.Abstractions;
using RnCore.Logging;

namespace Rn.Timerr.Services;

interface IJobRunnerService
{
  Task RunJobsAsync();
}

class JobRunnerService : IJobRunnerService
{
  private readonly ILoggerAdapter<JobRunnerService> _logger;
  private readonly IDateTimeAbstraction _dateTime;
  private readonly List<IRunnableJob> _jobs;

  public JobRunnerService(
    ILoggerAdapter<JobRunnerService> logger,
    IDateTimeAbstraction dateTime,
    IEnumerable<IRunnableJob> runnableJobs)
  {
    _logger = logger;
    _dateTime = dateTime;
    _jobs = runnableJobs.ToList();
  }

  public async Task RunJobsAsync()
  {
    if (_jobs.Count == 0)
      return;

    foreach (var job in _jobs)
    {
      if(!job.CanRun(_dateTime.Now))
        continue;

      await job.RunAsync(new JobConfiguration());
    }

    _logger.LogInformation("Completed tick...");
  }
}
