using Dapper;
using Rn.Timerr.Models.Entities;

namespace Rn.Timerr.Repos;

interface ISshCommandsActionsRepo
{
  Task<List<SshCommandsActionEntity>> GetEnabledCommandActions(string host);
}

class SshCommandsActionsRepo : ISshCommandsActionsRepo
{
  private readonly IConnectionFactory _connectionFactory;
  public const string TableName = "SshCommandsActions";

  public SshCommandsActionsRepo(IConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<List<SshCommandsActionEntity>> GetEnabledCommandActions(string host)
  {
    var query = @$"SELECT act.*
      FROM `{SshCommandsRepo.TableName}` cmd
      INNER JOIN `{TableName}` act ON cmd.`JobID` = act.`JobID`
      WHERE cmd.`Host` = '{host}'
	      AND cmd.`Enabled` = 1
      ORDER BY cmd.`CommandID`, act.`RunOrder` ASC";

    await using var conn = _connectionFactory.GetConnection();
    return (await conn.QueryAsync<SshCommandsActionEntity>(query)).ToList();
  }
}
