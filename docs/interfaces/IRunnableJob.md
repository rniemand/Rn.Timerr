# IRunnableJob

/ [Interfaces](./interfaces/README.md) / IRunnableJob

```cs
interface IRunnableJob
{
  string Name { get; }
  string ConfigKey { get; }

  bool CanRun(RunningJobOptions options);

  Task<RunningJobResult> RunAsync(RunningJobOptions options);
}
```
