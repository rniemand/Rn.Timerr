using Dapper;
using Rn.Timerr.Models.Entities;

namespace Rn.Timerr.Repos;

interface ISshCommandsRepo
{
  Task<List<SshCommandEntity>> GetEnabledCommands(string host);
}

class SshCommandsRepo : ISshCommandsRepo
{
  private readonly IConnectionFactory _connectionFactory;
  public const string TableName = "SshCommands";

  public SshCommandsRepo(IConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<List<SshCommandEntity>> GetEnabledCommands(string host)
  {
    var query = @$"SELECT *
    FROM `{TableName}`
    WHERE `Enabled` = 1
      AND `Host` = '{host}'";

    await using var conn = _connectionFactory.GetConnection();
    return (await conn.QueryAsync<SshCommandEntity>(query)).ToList();
  }
}
