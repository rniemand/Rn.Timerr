using Rn.Timerr.Jobs;
using Rn.Timerr.Models;
using Rn.Timerr.Providers;
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
  private readonly IJobConfigProvider _jobConfigProvider;
  private readonly List<IRunnableJob> _jobs;

  public JobRunnerService(
    ILoggerAdapter<JobRunnerService> logger,
    IDateTimeAbstraction dateTime,
    IJobConfigProvider jobConfigProvider,
    IEnumerable<IRunnableJob> runnableJobs)
  {
    _logger = logger;
    _dateTime = dateTime;
    _jobConfigProvider = jobConfigProvider;
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

      await job.RunAsync(new JobOptions
      {
        Config = _jobConfigProvider.GetJobConfig(job.ConfigKey),
        CurrentDateTime = _dateTime.Now
      });
    }

    _logger.LogInformation("Completed tick...");
  }
}
