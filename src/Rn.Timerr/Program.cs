using Microsoft.Extensions.DependencyInjection;
using Rn.Timerr;
using Rn.Timerr.Services;

var jobRunner = DIContainer.Services.GetRequiredService<IJobRunnerService>();

while (true)
{
  await jobRunner.RunJobsAsync();
  Console.Write(".");
  await Task.Delay(10 * 1000);
}
