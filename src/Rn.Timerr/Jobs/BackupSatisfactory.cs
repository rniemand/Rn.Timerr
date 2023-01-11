using System.IO.Compression;
using System.Text.RegularExpressions;
using Rn.Timerr.Attributes;
using Rn.Timerr.Enums;
using Rn.Timerr.Exceptions;
using Rn.Timerr.Models;
using Rn.Timerr.Utils;
using RnCore.Abstractions;
using RnCore.Logging;

namespace Rn.Timerr.Jobs;

class BackupSatisfactory : IRunnableJob
{
  public string Name => nameof(BackupSatisfactory);
  public string ConfigKey => nameof(BackupSatisfactory);

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

  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
  {
    var config = RunningJobUtils.MapConfiguration<Config>(options);
    if (!config.BackupFileName.EndsWith(".zip"))
      config.BackupFileName += ".zip";

    // Validate configuration
    var validationOutcome = RunningJobUtils.ValidateConfig(config);
    if (!validationOutcome.Success)
      return new RunningJobResult().WithError(validationOutcome.ValidationError);

    if (!ValidateDestinations(config))
      return new RunningJobResult(JobOutcome.Failed);

    await Task.CompletedTask;
    return BackupGameFiles(options, config);
  }


  // Game backup methods
  private RunningJobResult BackupGameFiles(RunningJobOptions options, Config config)
  {
    ManageGameSaves(config);

    var fileName = _path.Combine(config.Destination, GenerateFileName(options, config));
    if (_file.Exists(fileName) && !config.OverwriteExisting)
    {
      _logger.LogInformation("File {path} already exists, skipping backup", fileName);
      return new RunningJobResult(JobOutcome.Succeeded);
    }

    if (_file.Exists(fileName))
    {
      _logger.LogInformation("Removing existing backup file: {file}", fileName);
      _file.Delete(fileName);
    }

    _logger.LogDebug("Backing up saved files to: {file}", fileName);
    ZipFile.CreateFromDirectory(config.SourcePath, fileName, CompressionLevel.Optimal, true);
    _logger.LogInformation("Completed: {path} ({size})", fileName, new FileInfo(fileName).Length);

    options.ScheduleNextRunInXMinutes(config.TickIntervalMin);
    return new RunningJobResult(JobOutcome.Succeeded);
  }

  private void ManageGameSaves(Config config)
  {
    if (!config.ManageSaves)
      return;

    var managedSavesDir = _path.Join(config.Destination, "managed-saves");
    EnsureManagedSavesDir(managedSavesDir);

    var manageableFiles = GetManageableFiles(config);
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

    foreach (var saveFile in saveFiles.Where(f => _file.Exists(f.FullName)).Select(x => x.FullName))
    {
      _logger.LogDebug("Removing redundant save file: {path}", saveFile);
      _file.Delete(saveFile);
    }
  }

  private static DateTime ToStartOfDay(DateTime date) => new(date.Year, date.Month, date.Day);

  private List<FileInfo> GetManageableFiles(Config config)
  {
    var filteredFiles = new List<string>();

    // ReSharper disable once LoopCanBeConvertedToQuery
    foreach (var saveFile in _directory.GetFiles(config.Destination, "*.zip", SearchOption.TopDirectoryOnly))
    {
      var fileName = saveFile.Replace("\\", "/").Split('/').Last();

      if (!config.ManageSavesRx.IsMatch(fileName))
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
      throw new RnTimerrException($"Unable to create directory: {path}");
  }

  private bool ValidateDestinations(Config config)
  {
    if (!_directory.Exists(config.SourcePath))
    {
      _logger.LogError("Unable to find save file source path: {path}", config.SourcePath);
      return false;
    }

    if (!_directory.Exists(config.Destination))
      _directory.CreateDirectory(config.Destination);

    if (!_directory.Exists(config.Destination))
    {
      _logger.LogError("Unable to create destination directory: {path}", config.Destination);
      return false;
    }

    return true;
  }

  private string GenerateFileName(RunningJobOptions runningJobConfig, Config config) => config.BackupFileName
    .Replace("{yyyy}", runningJobConfig.JobStartTime.Year.ToString("D"))
    .Replace("{mm}", runningJobConfig.JobStartTime.Month.ToString("D").PadLeft(2, '0'))
    .Replace("{dd}", runningJobConfig.JobStartTime.Day.ToString("D").PadLeft(2, '0'))
    .Replace("{hh}", runningJobConfig.JobStartTime.Hour.ToString("D").PadLeft(2, '0'))
    .Replace("{mm}", runningJobConfig.JobStartTime.Minute.ToString("D").PadLeft(2, '0'))
    .Replace("{ss}", runningJobConfig.JobStartTime.Second.ToString("D").PadLeft(2, '0'));


  // Supporting classes
  class Config
  {
    [StringConfig("Source")]
    [StringValidator]
    public string SourcePath { get; set; } = string.Empty;

    [StringConfig("Destination")]
    [StringValidator]
    public string Destination { get; set; } = string.Empty;

    [StringConfig("BackupFileName")]
    [StringValidator]
    public string BackupFileName { get; set; } = string.Empty;

    [IntConfig("TickIntervalMin", Fallback = 10)]
    [IntValidator(10)]
    public int TickIntervalMin { get; set; } = 10;

    [BoolConfig("OverwriteExisting")]
    public bool OverwriteExisting { get; set; }

    [BoolConfig("ManageSaves")]
    public bool ManageSaves { get; set; }

    [RegexConfig("ManageSaveRx", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    public Regex ManageSavesRx { get; set; } = new(".*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
  }
}
