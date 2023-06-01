namespace Rn.Timerr.Models.Entities;

class SshCommandEntity
{
  public int CommandID { get; set; }
  public bool Enabled { get; set; }
  public string Host { get; set; } = string.Empty;
  public string CredentialName { get; set; } = string.Empty;
  public string JobID { get; set; } = string.Empty;
  public string JobName { get; set; } = string.Empty;
  public string ScheduleExpression { get; set; } = string.Empty;
  public DateTimeOffset LastRun { get; set; } = DateTime.MinValue;
  public DateTimeOffset NextRun { get; set; } = DateTime.MinValue;
}
