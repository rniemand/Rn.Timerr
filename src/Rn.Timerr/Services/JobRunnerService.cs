using Microsoft.Extensions.Configuration;
using Rn.Timerr.Jobs;
using Rn.Timerr.Models;
using RnCore.Abstractions;

namespace Rn.Timerr.Services;

interface IJobRunnerService
{
  Task RunJobsAsync();
}

class JobRunnerService : IJobRunnerService
{
  private readonly IDateTimeAbstraction _dateTime;
  private readonly List<IRunnableJob> _jobs;
  private readonly IConfigService _configService;
  private readonly IStateService _stateService;
  private readonly string _host;

  public JobRunnerService(
    IDateTimeAbstraction dateTime,
    IEnumerable<IRunnableJob> runnableJobs,
    IConfigService configService,
    IStateService stateService,
    IConfiguration configuration)
  {
    _dateTime = dateTime;
    _configService = configService;
    _stateService = stateService;
    _jobs = runnableJobs.ToList();

    // TODO: Add host info provider
    _host = configuration.GetValue<string>("RnTimerr:Host") ?? string.Empty;
    if (string.IsNullOrWhiteSpace(_host))
      throw new Exception("You need to define: 'RnTimerr:Host'");
  }

  public async Task RunJobsAsync()
  {
    if (_jobs.Count == 0)
      return;

    foreach (var job in _jobs)
    {
      var jobOptions = new JobOptions(job.ConfigKey, _host)
      {
        Config = await _configService.GetJobConfig(job.ConfigKey),
        State = await _stateService.GetJobStateAsync(job.ConfigKey),
        JobStartTime = _dateTime.Now
      };

      if (!job.CanRun(jobOptions))
        continue;

      await job.RunAsync(jobOptions);
      await _stateService.PersistStateAsync(jobOptions);
    }
  }
}
