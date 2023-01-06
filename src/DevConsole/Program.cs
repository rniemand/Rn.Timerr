using Microsoft.Extensions.DependencyInjection;
using Rn.Timerr;
using Rn.Timerr.Services;
using RnCore.Mailer.Builders;
using RnCore.Mailer.Config;
using RnCore.Mailer.Factories;

//var templateHelper = DIContainer.Services.GetRequiredService<IMailTemplateHelper>();
//var mailConfig = DIContainer.Services.GetRequiredService<RnMailConfig>();

//var mailMessage = new MailMessageBuilder()
//  .WithTo("niemand.richard@gmail.com")
//  .WithSubject("Testing email code")
//  .WithFrom(mailConfig)
//  .WithHtmlBody(templateHelper.GetTemplateBuilder("test"))
//  .Build();

//await DIContainer.Services
//  .GetRequiredService<IRnMailUtilsFactory>()
//  .CreateSmtpClient()
//  .SendMailAsync(mailMessage);

await DIContainer.Services
  .GetRequiredService<IJobRunnerService>()
  .RunJobsAsync();


Console.WriteLine();
Console.WriteLine();
