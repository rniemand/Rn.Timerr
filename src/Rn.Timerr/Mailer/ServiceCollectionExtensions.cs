using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RnCore.Logging;

namespace Rn.Timerr.Mailer;

static class ServiceCollectionExtensions
{
  public static IServiceCollection AddRnMailUtils(this IServiceCollection services, IConfiguration configuration)
  {
    services.TryAddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));

    return services
      .AddSingleton(BindConfig(configuration))
      .AddSingleton<IRnMailUtilsFactory, RnMailUtilsFactory>()
      .AddSingleton<IMailTemplateProvider, MailTemplateProvider>()
      .AddSingleton<IMailTemplateHelper, MailTemplateHelper>();
  }

  private static RnMailConfig BindConfig(IConfiguration configuration)
  {
    var boundConfig = new RnMailConfig();
    var configSection = configuration.GetSection("Rn.MailUtils");

    if (!configSection.Exists())
      return boundConfig;

    configSection.Bind(boundConfig);
    return boundConfig;
  }
}
