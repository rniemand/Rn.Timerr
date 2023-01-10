using Newtonsoft.Json;
using Rn.Timerr.Models;
using Rn.Timerr.Models.Config;
using Rn.Timerr.Repos;
using RnCore.Logging;

namespace Rn.Timerr.Services;

interface ICredentialsService
{
  Task<SshCredentials> GetSshCredentials(string name);
}

class CredentialsService : ICredentialsService
{
  private readonly ILoggerAdapter<CredentialsService> _logger;
  private readonly ICredentialsRepo _credentialsRepo;
  private readonly RnTimerrConfig _config;

  public CredentialsService(ILoggerAdapter<CredentialsService> logger,
    ICredentialsRepo credentialsRepo,
    RnTimerrConfig config)
  {
    _logger = logger;
    _credentialsRepo = credentialsRepo;
    _config = config;
  }


  // Interface methods
  public async Task<SshCredentials> GetSshCredentials(string name)
  {
    var dbCredentials = await _credentialsRepo.GetCredentials(_config.Host, name);
    if (dbCredentials is null)
    {
      _logger.LogError("Unable to find credentials '{name}'", name);
      return new SshCredentials();
    }

    try
    {
      var sshCredentials = JsonConvert.DeserializeObject<SshCredentials>(dbCredentials.Credentials);
      if (sshCredentials is null)
      {
        _logger.LogError("Unable to parse ssh credentials '{name}'", name);
        return new SshCredentials();
      }

      return sshCredentials;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to parse JSON as ssh credentials '{name}'", name);
      return new SshCredentials();
    }
  }
}
