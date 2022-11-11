using Rn.Timerr.Models;
using Rn.Timerr.Models.Config;
using Rn.Timerr.Repo;

namespace Rn.Timerr.Services;

interface IJobConfigService
{
  Task<JobConfig> GetJobConfig(string category);
}

class JobConfigService : IJobConfigService
{
  private readonly IConfigRepo _configRepo;
  private readonly RnTimerrConfig _config;

  public JobConfigService(IConfigRepo configRepo, RnTimerrConfig config)
  {
    _configRepo = configRepo;
    _config = config;
  }

  public async Task<JobConfig> GetJobConfig(string category)
  {
    var config = await _configRepo.GetAllConfigAsync(category, _config.Host);
    return new JobConfig(config);
  }
}
