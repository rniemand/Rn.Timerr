using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using System.Text;

namespace Rn.Timerr.Mailer;

[ExcludeFromCodeCoverage]
public class MailMessageBuilder
{
  private readonly MailMessage _mailMessage = new();
  private static readonly Encoding DefaultEncoding = Encoding.UTF8;
  private MailTemplateBuilder? _builder;

  public MailMessageBuilder WithFrom(string address, string displayName, Encoding encoding)
  {
    _mailMessage.From = new MailAddress(address, displayName, encoding);
    return this;
  }

  public MailMessageBuilder WithFrom(string address, string displayName) =>
    WithFrom(address, displayName, DefaultEncoding);

  public MailMessageBuilder WithFrom(string address) =>
    WithFrom(address, address);

  public MailMessageBuilder WithFrom(RnMailConfig config)
  {
    var encoding = config.Encoding ?? DefaultEncoding;

    // ReSharper disable once ConvertIfStatementToReturnStatement
    if (!string.IsNullOrWhiteSpace(config.FromName))
      return WithFrom(config.FromAddress, config.FromName, encoding);

    return WithFrom(config.FromAddress, config.FromAddress, encoding);
  }

  public MailMessageBuilder WithTo(string address, string displayName, Encoding encoding)
  {
    _mailMessage.To.Add(new MailAddress(address, displayName, encoding));
    return this;
  }

  public MailMessageBuilder WithTo(string address, string displayName) =>
    WithTo(address, displayName, DefaultEncoding);

  public MailMessageBuilder WithTo(string address) =>
    WithTo(address, address);

  public MailMessageBuilder WithSubject(string subject, Encoding encoding)
  {
    _mailMessage.Subject = subject;
    _mailMessage.SubjectEncoding = encoding;
    return this;
  }

  public MailMessageBuilder WithSubject(string subject) =>
    WithSubject(subject, DefaultEncoding);

  public MailMessageBuilder WithHtmlBody(string html, Encoding encoding)
  {
    _mailMessage.Body = html;
    _mailMessage.IsBodyHtml = true;
    _mailMessage.BodyEncoding = encoding;
    return this;
  }

  public MailMessageBuilder WithHtmlBody(string html) =>
    WithHtmlBody(html, DefaultEncoding);

  public MailMessageBuilder WithHtmlBody(MailTemplateBuilder builder)
  {
    _builder = builder;
    return this;
  }

  public MailMessage Build()
  {
    if (_builder is null)
      return _mailMessage;

    WithHtmlBody(_builder.AddPlaceholders(new Dictionary<string, object>
    {
      {"mail.subject", _mailMessage.Subject},
      {"mail.fromAddress", _mailMessage.From?.Address ?? string.Empty},
      {"mail.fromName", _mailMessage.From?.DisplayName ?? string.Empty},
      {"mail.toAddress", _mailMessage.To.FirstOrDefault()?.Address ?? string.Empty},
      {"mail.toName", _mailMessage.To.FirstOrDefault()?.DisplayName ?? string.Empty},
      {"mail.date", DateTime.Now}
    }).Process());

    return _mailMessage;
  }
}

