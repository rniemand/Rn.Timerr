using Renci.SshNet;
using Rn.Timerr.Exceptions;
using Rn.Timerr.Models;
using RnCore.Logging;

namespace Rn.Timerr.Utils;

interface ISshClientWrapper
{
  void RunSshCommand(string commandText, bool throwOnError = true);
}

class SshClientWrapper : ISshClientWrapper
{
  private readonly ILoggerAdapter<SshClientWrapper> _logger;
  private readonly SshClient? _client;

  public SshClientWrapper(ILoggerAdapter<SshClientWrapper> logger,
    SshCredentials credentials,
    string credentialsName)
  {
    _logger = logger;
    
    if (!credentials.IsValid())
    {
      _logger.LogError("Invalid credentials provided for '{name}'", credentialsName);
      return;
    }

    _client = new SshClient(new ConnectionInfo(
      credentials.Host,
      credentials.Port,
      credentials.User,
      new PasswordAuthenticationMethod(credentials.User, credentials.Pass))
    );

    _client.Connect();
  }


  // Interface methods
  public void RunSshCommand(string commandText, bool throwOnError = true)
  {
    if (_client is null || !_client.IsConnected)
      return;

    _logger.LogDebug("Running SSH command: {cmd}", commandText);
    var commandOutput = _client.RunCommand(commandText);

    if (string.IsNullOrWhiteSpace(commandOutput.Error) || !throwOnError)
      return;

    _logger.LogError("Error running command '{cmd}': {error}", commandOutput, commandOutput.Error);
    throw new RnTimerrException($"ssh command error: {commandOutput.Error}");
  }
}
