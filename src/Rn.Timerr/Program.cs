// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Rn.Timerr;
using RnCore.Logging;

var logger = DIContainer.Services.GetRequiredService<ILoggerAdapter<Program>>();


logger.LogInformation("Hello world");
