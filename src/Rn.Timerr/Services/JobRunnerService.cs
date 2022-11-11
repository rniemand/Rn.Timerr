using Rn.Timerr.Enums;
using Rn.Timerr.Jobs;
using Rn.Timerr.Models;
using Rn.Timerr.Models.Config;
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
  private readonly IJobConfigService _jobConfigService;
  private readonly IJobStateService _jobStateService;
  private readonly RnTimerrConfig _config;

  public JobRunnerService(
    ILoggerAdapter<JobRunnerService> logger,
    IDateTimeAbstraction dateTime,
    IEnumerable<IRunnableJob> runnableJobs,
    IJobConfigService jobConfigService,
    IJobStateService jobStateService,
    RnTimerrConfig config)
  {
    _dateTime = dateTime;
    _jobConfigService = jobConfigService;
    _jobStateService = jobStateService;
    _config = config;
    _logger = logger;
    _jobs = runnableJobs.ToList();
  }

  public async Task RunJobsAsync()
  {
    if (_jobs.Count == 0)
      return;

    foreach (var job in _jobs)
    {
      var jobOptions = new RunningJobOptions(job.ConfigKey, _config.Host)
      {
        Config = await _jobConfigService.GetJobConfig(job.ConfigKey),
        State = await _jobStateService.GetJobStateAsync(job.ConfigKey),
        JobStartTime = _dateTime.Now
      };

      if (!job.CanRun(jobOptions))
        continue;

      _logger.LogDebug("Running Job: {name}", job.Name);
      var jobResult = await job.RunAsync(jobOptions);

      if (jobResult.Outcome != JobOutcome.Succeeded)
      {
        _logger.LogWarning("Job {name} did not complete successfully!", job.Name);
        return;
      }

      await _jobStateService.PersistStateAsync(jobOptions);
    }
  }
}
