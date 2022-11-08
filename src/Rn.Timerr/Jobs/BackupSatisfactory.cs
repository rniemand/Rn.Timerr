using System.IO.Compression;
using Rn.Timerr.Enums;
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

  public bool CanRun(DateTime currentTime)
  {
    return true;
  }

  public async Task<JobOutcome> RunAsync(JobOptions jobConfig)
  {
    SetConfiguration(jobConfig);

    if (!ValidateDestinations())
      return new JobOutcome(JobState.Failed);

    var fileName = _path.Combine(_destPath, GenerateFileName(jobConfig));
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
    .Replace("{yyyy}", jobConfig.CurrentDateTime.Year.ToString("D"))
    .Replace("{mm}", jobConfig.CurrentDateTime.Month.ToString("D").PadLeft(2, '0'))
    .Replace("{dd}", jobConfig.CurrentDateTime.Day.ToString("D").PadLeft(2, '0'))
    .Replace("{hh}", jobConfig.CurrentDateTime.Hour.ToString("D").PadLeft(2, '0'))
    .Replace("{mm}", jobConfig.CurrentDateTime.Minute.ToString("D").PadLeft(2, '0'))
    .Replace("{ss}", jobConfig.CurrentDateTime.Second.ToString("D").PadLeft(2, '0'));
}
