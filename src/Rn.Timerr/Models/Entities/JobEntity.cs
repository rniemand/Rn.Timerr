namespace Rn.Timerr.Models.Entities;

class JobEntity
{
  public int JobID { get; set; }
  public bool Enabled { get; set; }
  public string Host { get; set; } = string.Empty;
  public string JobName { get; set; } = string.Empty;
  public DateTime NextRun { get; set; } = DateTime.MinValue;
  public DateTime LastRun { get; set; } = DateTime.MinValue;
}
