using Dapper;
using Rn.Timerr.Models.Entities;

namespace Rn.Timerr.Repos;

interface ICredentialsRepo
{
  Task<CredentialsEntity?> GetCredentials(string host, string name);
}

class CredentialsRepo : ICredentialsRepo
{
  private readonly IConnectionFactory _connectionFactory;
  public const string TableName = "Credentials";

  public CredentialsRepo(IConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }


  // Interface methods
  public async Task<CredentialsEntity?> GetCredentials(string host, string name)
  {
    var query = @$"SELECT *
    FROM `{TableName}`
    WHERE `Deleted` = 0
      AND `Host` = '{host}'
      AND `Name` = '{name}'";

    await using var conn = _connectionFactory.GetConnection();
    return (await conn.QueryAsync<CredentialsEntity>(query)).FirstOrDefault();
  }
}
