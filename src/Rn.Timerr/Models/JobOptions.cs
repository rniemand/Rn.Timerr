namespace Rn.Timerr.Models;

class JobOptions
{
  public string ConfigKey { get; set; }
  public string Host { get; set; }
  public JobConfig Config { get; set; } = new();
  public DateTime JobStartTime { get; set; }
  public RunningJobState State { get; set; }

  public JobOptions(string configKey, string host)
  {
    ConfigKey = configKey;
    Host = host;
    State = new RunningJobState(configKey, host);
  }
}
