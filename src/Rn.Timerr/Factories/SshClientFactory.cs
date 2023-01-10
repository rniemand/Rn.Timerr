using Microsoft.Extensions.DependencyInjection;
using Rn.Timerr.Services;
using Rn.Timerr.Utils;
using RnCore.Logging;

namespace Rn.Timerr.Factories;

interface ISshClientFactory
{
  Task<ISshClientWrapper> GetSshClient(string credentialsName);
}

class SshClientFactory : ISshClientFactory
{
  private readonly ICredentialsService _credentialsService;
  private readonly IServiceProvider _serviceProvider;

  public SshClientFactory(ICredentialsService credentialsService, IServiceProvider serviceProvider)
  {
    _credentialsService = credentialsService;
    _serviceProvider = serviceProvider;
  }


  // Interface methods
  public async Task<ISshClientWrapper> GetSshClient(string credentialsName)
  {
    var sshCredentials = await _credentialsService.GetSshCredentials(credentialsName);

    return new SshClientWrapper(
      _serviceProvider.GetRequiredService<ILoggerAdapter<SshClientWrapper>>(),
      sshCredentials,
      credentialsName);
  }
}
