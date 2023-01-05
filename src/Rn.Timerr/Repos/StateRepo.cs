using Dapper;
using Rn.Timerr.Models.Entities;

namespace Rn.Timerr.Repos;

interface IStateRepo
{
  Task<List<StateEntity>> GetAllStateAsync(string category, string host);
  Task<int> AddEntryAsync(StateEntity entry);
  Task<int> UpdateEntityAsync(StateEntity entry);
}

class StateRepo : IStateRepo
{
  private readonly IConnectionFactory _connectionFactory;

  public StateRepo(IConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<List<StateEntity>> GetAllStateAsync(string category, string host)
  {
    const string query = @"SELECT *
    FROM `State` s
    WHERE s.`Category` = @Category
      AND (s.`Host` = '*' OR s.`Host` = @Host)";

    await using var conn = _connectionFactory.GetConnection();

    var queryParams = new
    {
      Category = category,
      Host = host
    };

    return (await conn.QueryAsync<StateEntity>(query, queryParams)).ToList();
  }

  public async Task<int> AddEntryAsync(StateEntity entry)
  {
    const string query = @"INSERT INTO `State`
	    (`Category`,`Key`,`Host`,`Type`,`Value`)
    VALUES
	    (@Category, @Key, @Host, @Type, @Value);";

    await using var conn = _connectionFactory.GetConnection();
    return await conn.ExecuteAsync(query, entry);
  }

  public async Task<int> UpdateEntityAsync(StateEntity entry)
  {
    const string query = @"UPDATE `State`
    SET
      `Value` = @Value
    WHERE
      `Category` = @Category AND
      `Key` = @Key AND
      `Host` = @Host";

    await using var conn = _connectionFactory.GetConnection();
    return await conn.ExecuteAsync(query, entry);
  }
}
