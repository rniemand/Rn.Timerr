using RnCore.Logging;
using System.Net;

namespace Rn.Timerr.Mailer;

public interface IRnMailUtilsFactory
{
  MailMessageBuilder CreateMessageBuilder();
  ISmtpClient CreateSmtpClient();
}

public class RnMailUtilsFactory : IRnMailUtilsFactory
{
  private readonly ILoggerAdapter<RnMailUtilsFactory> _logger;
  private readonly RnMailConfig _mailConfig;

  public RnMailUtilsFactory(ILoggerAdapter<RnMailUtilsFactory> logger, RnMailConfig mailConfig)
  {
    _logger = logger;
    _mailConfig = mailConfig;
  }

  public MailMessageBuilder CreateMessageBuilder() =>
    new MailMessageBuilder().WithFrom(_mailConfig);

  public ISmtpClient CreateSmtpClient()
  {
    var smtpClient = new SmtpClientWrapper(_mailConfig.Host, _mailConfig.Port)
    {
      DeliveryFormat = _mailConfig.DeliveryFormat,
      DeliveryMethod = _mailConfig.DeliveryMethod,
      EnableSsl = _mailConfig.EnableSsl,
      PickupDirectoryLocation = null,
      TargetName = null,
      Timeout = _mailConfig.Timeout,
      UseDefaultCredentials = false
    };

    if (_mailConfig.HasCredentials())
    {
      smtpClient.Credentials = new NetworkCredential(_mailConfig.Username, _mailConfig.Password);
    }

    _logger.LogDebug("Created new instance for: {host}", _mailConfig.Host);
    return smtpClient;
  }
}
