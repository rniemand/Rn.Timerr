using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using System.Net;

namespace Rn.Timerr.Mailer;

public interface ISmtpClient
{
  SmtpDeliveryFormat DeliveryFormat { get; set; }
  SmtpDeliveryMethod DeliveryMethod { get; set; }
  bool EnableSsl { get; set; }
  string? PickupDirectoryLocation { get; set; }
  string? TargetName { get; set; }
  int Timeout { get; set; }
  bool UseDefaultCredentials { get; set; }
  ICredentialsByHost? Credentials { get; set; }
  string? Host { get; }
  int? Port { get; }

  Task SendMailAsync(MailMessage message);
  Task SendMailAsync(MailMessage message, CancellationToken cancellationToken);
  Task SendMailAsync(string from, string recipients, string? subject, string? body);
  Task SendMailAsync(string from, string recipients, string? subject, string? body, CancellationToken cancellationToken);
}

[ExcludeFromCodeCoverage]
public class SmtpClientWrapper : ISmtpClient
{
  public SmtpDeliveryFormat DeliveryFormat
  {
    get => _smtpClient.DeliveryFormat;
    set => _smtpClient.DeliveryFormat = value;
  }

  public SmtpDeliveryMethod DeliveryMethod
  {
    get => _smtpClient.DeliveryMethod;
    set => _smtpClient.DeliveryMethod = value;
  }

  public bool EnableSsl
  {
    get => _smtpClient.EnableSsl;
    set => _smtpClient.EnableSsl = value;
  }

  public string? PickupDirectoryLocation
  {
    get => _smtpClient.PickupDirectoryLocation;
    set => _smtpClient.PickupDirectoryLocation = value;
  }

  public string? TargetName
  {
    get => _smtpClient.TargetName;
    set => _smtpClient.TargetName = value;
  }

  public int Timeout
  {
    get => _smtpClient.Timeout;
    set => _smtpClient.Timeout = value;
  }

  public bool UseDefaultCredentials
  {
    get => _smtpClient.UseDefaultCredentials;
    set => _smtpClient.UseDefaultCredentials = value;
  }

  public ICredentialsByHost? Credentials
  {
    get => _smtpClient.Credentials;
    set => _smtpClient.Credentials = value;
  }

  public string? Host { get; private set; } = string.Empty;

  public int? Port { get; private set; } = 0;

  private readonly SmtpClient _smtpClient;


  // Constructors
  public SmtpClientWrapper()
  {
    _smtpClient = new SmtpClient();
  }

  public SmtpClientWrapper(string host)
  {
    Host = host;
    _smtpClient = new SmtpClient(host);
  }

  public SmtpClientWrapper(string host, int port)
  {
    Host = host;
    Port = port;
    _smtpClient = new SmtpClient(host, port);
  }


  // Methods
  public async Task SendMailAsync(MailMessage message) =>
    await _smtpClient.SendMailAsync(message);

  public async Task SendMailAsync(MailMessage message, CancellationToken cancellationToken) =>
    await _smtpClient.SendMailAsync(message, cancellationToken);

  public async Task SendMailAsync(string from, string recipients, string? subject, string? body) =>
    await _smtpClient.SendMailAsync(from, recipients, subject, body);

  public async Task SendMailAsync(string from, string recipients, string? subject, string? body, CancellationToken cancellationToken) =>
    await _smtpClient.SendMailAsync(from, recipients, subject, body, cancellationToken);
}
