using Microsoft.Extensions.Configuration;
using Rn.Timerr.Extensions;
using Rn.Timerr.Models;
using Rn.Timerr.Repo;
using RnCore.Logging;

namespace Rn.Timerr.Services;

interface IStateService
{
  Task<RunningJobState> GetJobStateAsync(string category);
  Task PersistStateAsync(JobOptions options);
}

class StateService : IStateService
{
  private readonly ILoggerAdapter<StateService> _logger;
  private readonly IStateRepo _stateRepo;
  private readonly string _host;

  public StateService(ILoggerAdapter<StateService> logger,
    IStateRepo stateRepo,
    IConfiguration configuration)
  {
    _logger = logger;
    _stateRepo = stateRepo;

    _host = configuration.GetValue<string>("RnTimerr:Host") ?? string.Empty;
    if (string.IsNullOrWhiteSpace(_host))
      throw new Exception("You need to define: 'RnTimerr:Host'");
  }

  public async Task<RunningJobState> GetJobStateAsync(string category)
  {
    var jobState = await _stateRepo.GetAllStateAsync(category, _host);
    return new RunningJobState(category, _host, jobState);
  }

  public async Task PersistStateAsync(JobOptions options)
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
