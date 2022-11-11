using Rn.Timerr.Extensions;
using Rn.Timerr.Models;
using Rn.Timerr.Models.Config;
using Rn.Timerr.Repo;
using RnCore.Logging;

namespace Rn.Timerr.Services;

interface IJobStateService
{
  Task<RunningJobState> GetJobStateAsync(string configKey);
  Task PersistStateAsync(RunningJobOptions options);
}

class JobStateService : IJobStateService
{
  private readonly ILoggerAdapter<JobStateService> _logger;
  private readonly IStateRepo _stateRepo;
  private readonly RnTimerrConfig _config;

  public JobStateService(ILoggerAdapter<JobStateService> logger,
    IStateRepo stateRepo,
    RnTimerrConfig config)
  {
    _logger = logger;
    _stateRepo = stateRepo;
    _config = config;
  }

  public async Task<RunningJobState> GetJobStateAsync(string configKey)
  {
    var jobState = await _stateRepo.GetAllStateAsync(configKey, _config.Host);
    return new RunningJobState(configKey, _config.Host, jobState);
  }

  public async Task PersistStateAsync(RunningJobOptions options)
  {
    var stateEntries = options.State.GetStateEntities();
    if (stateEntries.Count == 0)
      return;

    _logger.LogInformation("Persisting {count} state entries", stateEntries.Count);
    var dbConfig = await _stateRepo.GetAllStateAsync(options.ConfigKey, options.Host);

    foreach (var entity in stateEntries)
    {
      var dbEntity = dbConfig.FirstOrDefault(e =>
        e.Category.IgnoreCaseEquals(entity.Category) &&
        e.Key.IgnoreCaseEquals(entity.Key) &&
        e.Host.IgnoreCaseEquals(entity.Host));

      if (dbEntity is null)
        await _stateRepo.AddEntryAsync(entity);
      else
        await _stateRepo.UpdateEntityAsync(entity);
    }
  }
}
