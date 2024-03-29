using Rn.Timerr.Enums;
using Rn.Timerr.Extensions;
using Rn.Timerr.Jobs;
using Rn.Timerr.Metrics;
using Rn.Timerr.Models;
using Rn.Timerr.Models.Config;
using Rn.Timerr.Models.Entities;
using Rn.Timerr.Repos;
using RnCore.Logging;
using RnCore.Metrics.Extensions;

namespace Rn.Timerr.Services;

interface IJobRunnerService
{
  Task RunJobsAsync();
}

class JobRunnerService : IJobRunnerService
{
  private readonly ILoggerAdapter<JobRunnerService> _logger;
  private readonly List<IRunnableJob> _jobs;
  private readonly IJobConfigService _jobConfigService;
  private readonly IJobStateService _jobStateService;
  private readonly RnCore.Metrics.IMetricsService _metricsService;
  private readonly IJobsRepo _jobsRepo;
  private readonly RnTimerrConfig _config;
  private readonly List<JobEntity> _enabledJobs = new();

  public JobRunnerService(
    ILoggerAdapter<JobRunnerService> logger,
    IEnumerable<IRunnableJob> runnableJobs,
    IJobConfigService jobConfigService,
    IJobStateService jobStateService,
    RnTimerrConfig config,
    IJobsRepo jobsRepo,
    RnCore.Metrics.IMetricsService metricsService)
  {
    _jobConfigService = jobConfigService;
    _jobStateService = jobStateService;
    _config = config;
    _jobsRepo = jobsRepo;
    _metricsService = metricsService;
    _logger = logger;
    _jobs = runnableJobs.ToList();
  }


  // Interface methods
  public async Task RunJobsAsync()
  {
    if (_jobs.Count == 0)
      return;

    await RefreshEnabledJobs();

    foreach (var job in _jobs)
    {
      // Skip over any disabled jobs
      if (!_enabledJobs.Any(x => x.JobName.IgnoreCaseEquals(job.ConfigKey)))
        continue;

      var metricBuilder = new JobMetricBuilder(job.ConfigKey)
        .WithWasRun(false)
        .WithJobSucceeded(false);

      try
      {
        using (metricBuilder.WithTiming())
        {
          // Fetch the job from the DB and see if we can run it now...
          var dbJob = await _jobsRepo.GetJobAsync(_config.Host, job.Name);
          if (!CanRunJobNow(dbJob))
            continue;

          // Load all available job options from the DB...
          _logger.LogInformation("Running Job: {name}", job.Name);
          var jobOptions = await GetJobOptionsAsync(job);
          var jobResult = await job.RunAsync(jobOptions);

          metricBuilder
            .WithWasRun(true)
            .WithOptionsCount(jobOptions.Config.GetOptionCount())
            .WithJobSucceeded(jobResult.Outcome == JobOutcome.Succeeded);

          if (jobResult.Outcome != JobOutcome.Succeeded)
          {
            _logger.LogWarning("Job {name} failed: {reason}", job.Name, jobResult.Error);
            return;
          }

          await _jobStateService.PersistStateAsync(dbJob, jobOptions);
        }
      }
      catch (Exception ex)
      {
        metricBuilder.WithException(ex);
      }
      finally
      {
        await _metricsService.SubmitAsync(metricBuilder);
      }
    }
  }


  // Internal methods
  private async Task RefreshEnabledJobs()
  {
    var builder = new ServiceMetricBuilder(nameof(JobRunnerService), nameof(RefreshEnabledJobs))
      .WithSuccess(true);

    try
    {
      using (builder.WithTiming())
      {
        _enabledJobs.Clear();
        _enabledJobs.AddRange(await _jobsRepo.GetJobsAsync(_config.Host));
      }
    }
    catch (Exception ex)
    {
      builder.WithException(ex);
    }
    finally
    {
      await _metricsService.SubmitAsync(builder);
    }
  }

  private async Task<RunningJobOptions> GetJobOptionsAsync(IRunnableJob job) =>
    new(job.ConfigKey, _config.Host)
    {
      Config = await _jobConfigService.GetJobConfig(job.ConfigKey),
      State = await _jobStateService.GetJobStateAsync(job.ConfigKey),
      JobStartTime = DateTimeOffset.Now
    };

  private static bool CanRunJobNow(JobEntity jobEntity) => DateTimeOffset.Now >= jobEntity.NextRun;
}
