using RnCore.Metrics.Builders;
using RnCore.Metrics.Models;

namespace Rn.Timerr.Metrics;

public class ServiceMetricBuilder : BaseMetricBuilder<ServiceMetricBuilder>
{
  private string _service = string.Empty;
  private string _method = string.Empty;

  public ServiceMetricBuilder()
    : base("service_call")
  {
    SetSuccess(true);
  }

  public ServiceMetricBuilder(string service, string method)
    : this()
  {
    ForService(service, method);
  }

  public ServiceMetricBuilder WithException(Exception ex)
  {
    SetException(ex);
    return this;
  }

  public ServiceMetricBuilder ForService(string service, string method)
  {
    _service = service;
    _method = method;
    return this;
  }

  public override RnMetric Build()
  {
    AddAction(m => { m.SetTag("service", _service, skipToLower: true); });
    AddAction(m => { m.SetTag("method", _method, skipToLower: true); });
    return base.Build();
  }
}
