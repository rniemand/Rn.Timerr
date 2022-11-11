using System.IO.Compression;
using System.Text.RegularExpressions;
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
  private Regex _managedSaveRx = new(".*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
  private int _tickIntervalMin = 10;
  private bool _overwriteExisting;
  private bool _manageSaves;

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

  // Interface methods
  public bool CanRun(JobOptions options)
  {
    if (!options.State.ContainsKey("NextRunTime"))
      return true;

    var nextRunTime = options.State.GetDateTimeValue("NextRunTime");
    if (nextRunTime > options.JobStartTime)
      return false;

    return true;
  }

  public async Task<JobOutcome> RunAsync(JobOptions options)
  {
    SetConfiguration(options);

    if (!ValidateDestinations())
      return new JobOutcome(JobState.Failed);

    await Task.CompletedTask;
    return BackupGameFiles(options);
  }

  // Game backup methods
  private JobOutcome BackupGameFiles(JobOptions options)
  {
    ManageGameSaves();

    var fileName = _path.Combine(_destPath, GenerateFileName(options));
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

    var nextRunTime = options.JobStartTime.AddMinutes(_tickIntervalMin);
    options.State.SetValue("NextRunTime", nextRunTime);
    _logger.LogDebug("Scheduled next tick for: {time}", nextRunTime);

    return new JobOutcome(JobState.Succeeded);
  }

  private void ManageGameSaves()
  {
    if (!_manageSaves)
      return;

    var managedSavesDir = _path.Join(_destPath, "managed-saves");
    EnsureManagedSavesDir(managedSavesDir);

    var manageableFiles = GetManageableFiles();
    if (manageableFiles.Count == 0)
      return;

    _logger.LogInformation("Found {count} file(s) to manage", manageableFiles.Count);

    var fileDateRanges = manageableFiles
      .OrderBy(f => f.LastWriteTime)
      .Select(f => ToStartOfDay(f.LastWriteTime))
      .Distinct()
      .ToList();

    foreach (var baseDate in fileDateRanges)
      ManageDaysSaveFiles(managedSavesDir, manageableFiles.Where(f => ToStartOfDay(f.LastWriteTime) == baseDate));
  }


  // Helper methods
  private void ManageDaysSaveFiles(string manageDir, IEnumerable<FileInfo> files)
  {
    var saveFiles = files
      .OrderBy(f => f.LastWriteTime)
      .ToList();

    if (saveFiles.Count == 0)
      return;

    var lastDailySaveFile = saveFiles.Last();
    var dateString = ToStartOfDay(lastDailySaveFile.LastWriteTime).ToString("yyyy-MM-dd");
    _logger.LogInformation("Selecting '{lastSave}' as daily save file for {date}", lastDailySaveFile.Name, dateString);

    var filePath = _path.Combine(manageDir, $"Satisfactory_Daily_{dateString}.zip");
    _file.Move(lastDailySaveFile.FullName, filePath, true);

    foreach (var saveFile in saveFiles.Where(f => _file.Exists(f.FullName)))
    {
      _logger.LogDebug("Removing redundant save file: {path}", saveFile.FullName);
      _file.Delete(saveFile.FullName);
    }
  }

  private static DateTime ToStartOfDay(DateTime date) => new(date.Year, date.Month, date.Day);

  private List<FileInfo> GetManageableFiles()
  {
    var filteredFiles = new List<string>();

    // ReSharper disable once LoopCanBeConvertedToQuery
    foreach (var saveFile in _directory.GetFiles(_destPath, "*.zip", SearchOption.TopDirectoryOnly))
    {
      var fileName = saveFile.Replace("\\", "/").Split('/').Last();

      if (!_managedSaveRx.IsMatch(fileName))
        continue;

      filteredFiles.Add(saveFile);
    }

    var startOfDay = ToStartOfDay(DateTime.Now);

    return filteredFiles
      .Select(filteredSaveFile => new FileInfo(filteredSaveFile))
      .Where(fileInfo => fileInfo.LastWriteTime < startOfDay)
      .ToList();
  }

  private void EnsureManagedSavesDir(string path)
  {
    if (_directory.Exists(path))
      return;

    _logger.LogInformation("Creating managed saves directory: {path}", path);
    _directory.CreateDirectory(path);

    if (!_directory.Exists(path))
      throw new Exception($"Unable to create directory: {path}");
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
    _manageSaves = jobConfig.Config.GetBoolValue("ManageSaves", false);

    var managedSaveRx = jobConfig.Config.GetStringValue("ManageSaveRx");
    if (!string.IsNullOrWhiteSpace(managedSaveRx))
      _managedSaveRx = new Regex(managedSaveRx, RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
