using Microsoft.Extensions.DependencyInjection;
using Rn.Timerr;
using Rn.Timerr.Services;

await DIContainer.Services
  .GetRequiredService<IJobRunnerService>()
  .RunJobsAsync();
