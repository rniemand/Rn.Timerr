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


  public void ScheduleNextRunInXHours(int hoursFromNow)
  {
    var nextRunTime = DateTimeOffset.Now.AddHours(hoursFromNow);
    State.SetValue(RnTimerrStatic.NextRunTime, nextRunTime);
  }

  public void ScheduleNextRunInXMinutes(int minutesFromNow)
  {
    var nextRunTime = DateTimeOffset.Now.AddMinutes(minutesFromNow);
    State.SetValue(RnTimerrStatic.NextRunTime, nextRunTime);
  }

  public void ScheduleNextRunUsingTemplate(DateTimeOffset date, string template)
  {
    var nextRunTime = DateTimeOffset.Parse(template
      .Replace("yyyy", date.Year.ToString())
      .Replace("MM", date.Month.ToString().PadLeft(2, '0'))
      .Replace("dd", date.Day.ToString().PadLeft(2, '0')));

    State.SetValue(RnTimerrStatic.NextRunTime, nextRunTime);
  }
}
