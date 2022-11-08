using Rn.Timerr.Enums;

namespace Rn.Timerr.Models;

class JobOutcome
{
  public JobState State { get; set; } = JobState.Failed;

  public JobOutcome() { }

  public JobOutcome(JobState state)
  {
    State = state;
  }
}
