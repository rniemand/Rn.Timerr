namespace Rn.Timerr.Models.Entities;

class SshCommandsActionEntity
{
  public int ActionID { get; set; }
  public string JobID { get; set; } = string.Empty;
  public int RunOrder { get; set; }
  public string Host { get; set; } = string.Empty;
  public bool StopOnError { get; set; }
  public string Command { get; set; } = string.Empty;
}
