using Microsoft.Extensions.DependencyInjection;
using Rn.Timerr;
using Rn.Timerr.Services;

var jobRunner = DIContainer.Services.GetRequiredService<IJobRunnerService>();

await jobRunner.RunJobsAsync();

