using Rn.Timerr.Jobs;
using Rn.Timerr.Models;
using Rn.Timerr.Providers;
using RnCore.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Rn.Timerr.Helpers;

namespace Rn.Timerr.Services;

interface IJobRunnerService
{
  Task RunJobsAsync();
}

class JobRunnerService : IJobRunnerService
{
  private readonly IDateTimeAbstraction _dateTime;
  private readonly IJobConfigProvider _jobConfigProvider;
  private readonly IJsonHelper _jsonHelper;
  private readonly IPathAbstraction _path;
  private readonly IFileAbstraction _file;
  private readonly List<IRunnableJob> _jobs;

  private readonly string _stateDir;
  private readonly Dictionary<string, Dictionary<string, object>> _jobStates = new();

  public JobRunnerService(
    IDateTimeAbstraction dateTime,
    IJobConfigProvider jobConfigProvider,
    IJsonHelper jsonHelper,
    IPathAbstraction path,
    IFileAbstraction file,
    IDirectoryAbstraction directory,
    IEnumerable<IRunnableJob> runnableJobs)
  {
    _dateTime = dateTime;
    _jobConfigProvider = jobConfigProvider;
    _jsonHelper = jsonHelper;
    _path = path;
    _file = file;
    _jobs = runnableJobs.ToList();

    // Ensure that we have a state directory to work with
    _stateDir = ExeRelativeDir("job-states");
    if (!directory.Exists(_stateDir))
      directory.CreateDirectory(_stateDir);
  }

  public async Task RunJobsAsync()
  {
    if (_jobs.Count == 0)
      return;

    foreach (var job in _jobs)
    {
      var jobOptions = new JobOptions
      {
        Config = _jobConfigProvider.GetJobConfig(job.ConfigKey),
        JobStartTime = _dateTime.Now,
        State = GetJobState(job)
      };

      if (!job.CanRun(jobOptions))
        continue;

      await job.RunAsync(jobOptions);
      PersistJobState(job, jobOptions);
    }
  }


  private void PersistJobState(IRunnableJob job, JobOptions jobOptions)
  {
    var stateFile = GenerateStateFileName(job);

    if (_file.Exists(stateFile))
      _file.Delete(stateFile);

    var rawJson = _jsonHelper.SerializeObject(jobOptions.State, true);
    _file.WriteAllText(stateFile, rawJson);
  }

  private Dictionary<string, object> GetJobState(IRunnableJob job)
  {
    if (_jobStates.ContainsKey(job.ConfigKey))
      return _jobStates[job.ConfigKey];

    var stateFile = GenerateStateFileName(job);

    if (!_file.Exists(stateFile))
    {
      var rawJson = _jsonHelper.SerializeObject(new Dictionary<string, object>(), true);
      _file.WriteAllText(stateFile, rawJson);
    }

    _jobStates[job.ConfigKey] = _jsonHelper.DeserializeObject<Dictionary<string, object>>(_file.ReadAllText(stateFile));
    return _jobStates[job.ConfigKey];
  }

  private static string ExeRelativeDir(string path)
  {
    path = ExeRelativeFile(path);
    if (!path.EndsWith('/'))
      path += '/';
    return path;
  }

  private static string ExeRelativeFile(string path) =>
    NormalizePath(Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path));

  [return: NotNullIfNotNull("path")]
  private static string? NormalizePath(string? path) => path?.Replace("\\", "/");

  private string GenerateStateFileName(IRunnableJob job) => _path.Combine(_stateDir, $"{job.ConfigKey}.json");
}
