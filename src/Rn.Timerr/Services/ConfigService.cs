using Microsoft.Extensions.Configuration;
using Rn.Timerr.Models;
using Rn.Timerr.Repo;
using RnCore.Logging;

namespace Rn.Timerr.Services;

interface IConfigService
{
  Task<JobConfig> GetJobConfig(string category);
}

class ConfigService : IConfigService
{
  private readonly ILoggerAdapter<ConfigService> _logger;
  private readonly IConfigRepo _configRepo;
  private readonly string _host;

  public ConfigService(ILoggerAdapter<ConfigService> logger,
    IConfigRepo configRepo,
    IConfiguration configuration)
  {
    _configRepo = configRepo;
    _logger = logger;

    _host = configuration.GetValue<string>("RnTimerr:Host") ?? string.Empty;
    if (string.IsNullOrWhiteSpace(_host))
      throw new Exception("You need to define: 'RnTimerr:Host'");
  }

  public async Task<JobConfig> GetJobConfig(string category)
  {
    var config = await _configRepo.GetAllConfigAsync(category, _host);
    return new JobConfig(config);
  }
}
