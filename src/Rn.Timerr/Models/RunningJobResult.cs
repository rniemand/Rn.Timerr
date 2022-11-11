using Rn.Timerr.Enums;

namespace Rn.Timerr.Models;

class RunningJobResult
{
  public JobOutcome Outcome { get; set; } = JobOutcome.Failed;

  public RunningJobResult() { }

  public RunningJobResult(JobOutcome outcome)
  {
    Outcome = outcome;
  }
}
