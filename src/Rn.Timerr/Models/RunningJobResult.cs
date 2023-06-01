using Rn.Timerr.Enums;

namespace Rn.Timerr.Models;

public class RunningJobResult
{
  public JobOutcome Outcome { get; set; } = JobOutcome.Failed;
  public string Error { get; set; } = string.Empty;

  public RunningJobResult() { }

  public RunningJobResult(JobOutcome outcome)
  {
    Outcome = outcome;
  }

  public RunningJobResult WithError(string error)
  {
    Error = error;
    return this;
  }

  public RunningJobResult AsSucceeded()
  {
    Outcome = JobOutcome.Succeeded;
    return this;
  }
}
