namespace Rn.Timerr.Models;

class RunningJobOptions
{
  public string ConfigKey { get; set; }
  public string Host { get; set; }
  public JobConfig Config { get; set; } = new();
  public DateTimeOffset JobStartTime { get; set; }
  public RunningJobState State { get; set; }

  public RunningJobOptions(string configKey, string host)
  {
    ConfigKey = configKey;
    Host = host;
    State = new RunningJobState(configKey, host);
  }
}
