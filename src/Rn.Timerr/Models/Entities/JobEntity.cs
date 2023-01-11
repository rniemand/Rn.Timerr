namespace Rn.Timerr.Models.Entities;

class JobEntity
{
  public int JobID { get; set; }
  public bool Enabled { get; set; }
  public string Host { get; set; } = string.Empty;
  public string JobName { get; set; } = string.Empty;
  public DateTimeOffset NextRun { get; set; } = DateTime.MinValue;
  public DateTimeOffset LastRun { get; set; } = DateTime.MinValue;
}
