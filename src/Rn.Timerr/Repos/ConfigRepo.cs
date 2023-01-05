using Dapper;
using Rn.Timerr.Models.Entities;

namespace Rn.Timerr.Repos;

interface IConfigRepo
{
  Task<List<ConfigEntity>> GetAllConfigAsync(string category, string host);
}

class ConfigRepo : IConfigRepo
{
  private readonly IConnectionFactory _connectionFactory;

  public ConfigRepo(IConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<List<ConfigEntity>> GetAllConfigAsync(string category, string host)
  {
    const string query = @"SELECT *
    FROM `Config` c
    WHERE c.`Category` = @Category
      AND (c.`Host` = '*' OR c.`Host` = @Host)";

    await using var conn = _connectionFactory.GetConnection();

    var queryParams = new
    {
      Category = category,
      Host = host
    };

    return (await conn.QueryAsync<ConfigEntity>(query, queryParams)).ToList();
  }
}
