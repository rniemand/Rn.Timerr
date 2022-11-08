using System.IO.Compression;
using Rn.Timerr.Enums;
using Rn.Timerr.Extensions;
using Rn.Timerr.Models;
using RnCore.Abstractions;
using RnCore.Logging;

namespace Rn.Timerr.Jobs;

class BackupSatisfactory : IRunnableJob
{
  public string Name => nameof(BackupSatisfactory);
  public string ConfigKey => nameof(BackupSatisfactory);

  private string _sourcePath = string.Empty;
  private string _destPath = string.Empty;
  private string _fileName = string.Empty;
  private int _tickIntervalMin = 10;
  private bool _overwriteExisting;

  private readonly ILoggerAdapter<BackupSatisfactory> _logger;
  private readonly IDirectoryAbstraction _directory;
  private readonly IFileAbstraction _file;
  private readonly IPathAbstraction _path;

  public BackupSatisfactory(ILoggerAdapter<BackupSatisfactory> logger,
    IDirectoryAbstraction directory,
    IFileAbstraction file,
    IPathAbstraction path)
  {
    _logger = logger;
    _directory = directory;
    _file = file;
    _path = path;
  }

  public bool CanRun(JobOptions jobOptions)
  {
    if (!jobOptions.State.HasStateKey("NextRunTime"))
      return true;

    var nextRunTime = jobOptions.State.GetDateTimeValue("NextRunTime");
    if (nextRunTime> jobOptions.JobStartTime)
      return false;

    return true;
  }

  public async Task<JobOutcome> RunAsync(JobOptions jobOptions)
  {
    SetConfiguration(jobOptions);

    if (!ValidateDestinations())
      return new JobOutcome(JobState.Failed);

    var fileName = _path.Combine(_destPath, GenerateFileName(jobOptions));
    if (_file.Exists(fileName) && !_overwriteExisting)
    {
      _logger.LogInformation("File {path} already exists, skipping backup", fileName);
      return new JobOutcome(JobState.Succeeded);
    }

    if (_file.Exists(fileName))
    {
      _logger.LogInformation("Removing existing backup file: {file}", fileName);
      _file.Delete(fileName);
    }

    _logger.LogDebug("Backing up saved files to: {file}", fileName);
    ZipFile.CreateFromDirectory(_sourcePath, fileName, CompressionLevel.Optimal, true);
    _logger.LogInformation("Completed: {path} ({size})", fileName, new FileInfo(fileName).Length);

    jobOptions.State["NextRunTime"] = jobOptions.JobStartTime.AddMinutes(_tickIntervalMin);
    _logger.LogDebug("Scheduled next tick for: {time}", jobOptions.State["NextRunTime"]);

    await Task.CompletedTask;
    return new JobOutcome(JobState.Succeeded);
  }

  private void SetConfiguration(JobOptions jobConfig)
  {
    if (!jobConfig.Config.HasStringValue("Source"))
      throw new Exception("Missing configuration for: Source");

    if (!jobConfig.Config.HasStringValue("Destination"))
      throw new Exception("Missing configuration for: Destination");

    if (!jobConfig.Config.HasStringValue("BackupFileName"))
      throw new Exception("Missing configuration for: BackupFileName");

    _sourcePath = jobConfig.Config.GetStringValue("Source");
    _destPath = jobConfig.Config.GetStringValue("Destination");
    _fileName = jobConfig.Config.GetStringValue("BackupFileName");
    _tickIntervalMin = jobConfig.Config.GetIntValue("TickIntervalMin", 10);
    _overwriteExisting = jobConfig.Config.GetBoolValue("OverwriteExisting", false);

    if (string.IsNullOrWhiteSpace(_sourcePath))
      throw new Exception("Source: must have a value");

    if (string.IsNullOrWhiteSpace(_destPath))
      throw new Exception("Destination: must have a value");

    if (string.IsNullOrWhiteSpace(_fileName))
      throw new Exception("BackupFileName: must have a value");

    if (!_fileName.EndsWith(".zip"))
      _fileName += ".zip";
  }

  private bool ValidateDestinations()
  {
    if (!_directory.Exists(_sourcePath))
    {
      _logger.LogError("Unable to find save file source path: {path}", _sourcePath);
      return false;
    }

    if (!_directory.Exists(_destPath))
      _directory.CreateDirectory(_destPath);

    if (!_directory.Exists(_destPath))
    {
      _logger.LogError("Unable to create destination directory: {path}", _destPath);
      return false;
    }

    return true;
  }

  private string GenerateFileName(JobOptions jobConfig) => _fileName
    .Replace("{yyyy}", jobConfig.JobStartTime.Year.ToString("D"))
    .Replace("{mm}", jobConfig.JobStartTime.Month.ToString("D").PadLeft(2, '0'))
    .Replace("{dd}", jobConfig.JobStartTime.Day.ToString("D").PadLeft(2, '0'))
    .Replace("{hh}", jobConfig.JobStartTime.Hour.ToString("D").PadLeft(2, '0'))
    .Replace("{mm}", jobConfig.JobStartTime.Minute.ToString("D").PadLeft(2, '0'))
    .Replace("{ss}", jobConfig.JobStartTime.Second.ToString("D").PadLeft(2, '0'));
}
