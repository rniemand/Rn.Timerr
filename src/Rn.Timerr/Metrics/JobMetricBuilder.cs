using RnCore.Metrics.Builders;
using RnCore.Metrics.Models;

namespace Rn.Timerr.Metrics;

public class JobMetricBuilder : BaseMetricBuilder<JobMetricBuilder>
{
  private string _jobName = string.Empty;
  private int _optionsCount = 0;
  private bool _wasRun = false;
  private bool _jobSucceeded = false;

  public JobMetricBuilder()
    : base("job")
  {
    SetSuccess(true);
  }

  public JobMetricBuilder(string jobName)
    : this()
  {
    WithJobName(jobName);
  }

  public JobMetricBuilder WithException(Exception ex)
  {
    SetException(ex);
    return this;
  }

  public JobMetricBuilder WithJobName(string jobName)
  {
    _jobName = jobName;
    return this;
  }

  public JobMetricBuilder WithOptionsCount(int count)
  {
    _optionsCount = count;
    return this;
  }

  public JobMetricBuilder WithWasRun(bool wasRun)
  {
    _wasRun = wasRun;
    return this;
  }

  public JobMetricBuilder WithJobSucceeded(bool succeeded)
  {
    _jobSucceeded = succeeded;
    return this;
  }

  public override RnMetric Build()
  {
    AddAction(m => { m.SetTag("job", _jobName, skipToLower: true); });
    AddAction(m => { m.SetField("options_count", _optionsCount); });
    AddAction(m => { m.SetField("was_run", _wasRun); });
    AddAction(m => { m.SetField("job_succeeded", _jobSucceeded); });
    return base.Build();
  }
}
