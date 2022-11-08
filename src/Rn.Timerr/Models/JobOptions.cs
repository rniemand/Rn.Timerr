namespace Rn.Timerr.Models;

class JobOptions
{
  public JobConfig Config { get; set; } = new();
  public DateTime JobStartTime { get; set; }
  public Dictionary<string, object> State { get; set; } = new();
}
