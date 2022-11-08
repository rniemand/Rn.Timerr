using Microsoft.Extensions.Configuration;
using Rn.Timerr.Models;
using RnCore.Logging;

namespace Rn.Timerr.Providers;

interface IJobConfigProvider
{
  JobConfig GetJobConfig(string configKey);
}

class JobConfigProvider : IJobConfigProvider
{
  private readonly ILoggerAdapter<JobConfigProvider> _logger;
  private readonly IConfiguration _configuration;

  public JobConfigProvider(ILoggerAdapter<JobConfigProvider> logger, IConfiguration configuration)
  {
    _logger = logger;
    _configuration = configuration;
  }

  public JobConfig GetJobConfig(string configKey)
  {
    var genConfigKey = $"JobConfig:{configKey}";
    var section = _configuration.GetSection(genConfigKey);

    if (!section.Exists())
    {
      _logger.LogWarning("Unable to find job configuration section: '{key}'", genConfigKey);
      return new JobConfig();
    }

    return new JobConfig(section);
  }
}
