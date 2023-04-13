using Rn.Timerr.Enums;
using Rn.Timerr.Factories;
using Rn.Timerr.Models;

namespace Rn.Timerr.Jobs;

class RunSshCommands : IRunnableJob
{
  public string Name => nameof(RunSshCommands);
  public string ConfigKey => nameof(RunSshCommands);

  private readonly ISshClientFactory _sshClientFactory;


  public RunSshCommands(ISshClientFactory sshClientFactory)
  {
    _sshClientFactory = sshClientFactory;
  }

  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
  {
    var jobOutcome = new RunningJobResult(JobOutcome.Succeeded);


    options.ScheduleNextRunInXMinutes(1);

    return jobOutcome;
  }
}
