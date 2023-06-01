using Renci.SshNet;
using Rn.Timerr.Enums;
using Rn.Timerr.Factories;
using Rn.Timerr.Models;
using Rn.Timerr.Models.Entities;
using Rn.Timerr.Repos;

namespace Rn.Timerr.Jobs;

//class RunSshCommands : IRunnableJob
//{
//  public string Name => nameof(RunSshCommands);
//  public string ConfigKey => nameof(RunSshCommands);

//  private readonly ISshClientFactory _sshClientFactory;
//  private readonly ISshCommandsRepo _sshCommandsRepo;
//  private readonly ISshCommandsActionsRepo _sshCommandsActionsRepo;
//  private readonly List<SshCommandEntity> _sshCommands = new();
//  private readonly List<SshCommandsActionEntity> _commandActionEntries = new();

//  public RunSshCommands(ISshClientFactory sshClientFactory,
//    ISshCommandsRepo sshCommandsRepo,
//    ISshCommandsActionsRepo sshCommandsActionsRepo)
//  {
//    _sshClientFactory = sshClientFactory;
//    _sshCommandsRepo = sshCommandsRepo;
//    _sshCommandsActionsRepo = sshCommandsActionsRepo;
//  }

//  public async Task<RunningJobResult> RunAsync(RunningJobOptions options)
//  {
//    await LoadEnabledJobsAsync(options);

//    var jobOutcome = new RunningJobResult(JobOutcome.Succeeded);

//    foreach (var command in GetRunnableCommands())
//      await RunCommandAsync(options, command);



//    options.ScheduleNextRunInXMinutes(1);

//    return jobOutcome;
//  }

//  // Internal methods
//  private async Task LoadEnabledJobsAsync(RunningJobOptions options)
//  {
//    if (_sshCommands.Count > 0) return;

//    var sshCommandEntities = await _sshCommandsRepo.GetEnabledCommands(options.Host);
//    _sshCommands.AddRange(sshCommandEntities);

//    if (_sshCommands.Count == 0)
//      throw new Exception("There are no enabled SSH Commands for this host");

//    await LoadCommandActionsAsync(options);
//  }

//  private async Task LoadCommandActionsAsync(RunningJobOptions options)
//  {
//    _commandActionEntries.AddRange(await _sshCommandsActionsRepo.GetEnabledCommandActions(options.Host));
//  }

//  private List<SshCommandEntity> GetRunnableCommands()
//  {
//    return _sshCommands.Where(x => x.NextRun <= DateTimeOffset.Now).ToList();
//  }

//  private List<SshCommandsActionEntity> GetCommandActions(SshCommandEntity command)
//  {
//    return _commandActionEntries.Where(x => x.JobID.Equals(command.JobID)).OrderBy(x => x.RunOrder).ToList();
//  }

//  private async Task RunCommandAsync(RunningJobOptions options, SshCommandEntity command)
//  {
//    var actions = GetCommandActions(command);
//    if (!actions.Any())
//      throw new Exception("This command has no actions to execute!");

//    var sshClient = await _sshClientFactory.GetSshClient(command.CredentialName);



//    Console.WriteLine();
//    Console.WriteLine();
//  }
//}
