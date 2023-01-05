using Dapper;
using Rn.Timerr.Models.Entities;

namespace Rn.Timerr.Repos;

interface IJobsRepo
{
  Task<List<JobEntity>> GetJobsAsync();
}

class JobsRepo : IJobsRepo
{
  private readonly IConnectionFactory _connectionFactory;
  public const string TableName = "Jobs";

  public JobsRepo(IConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }


  // Interface methods
  public async Task<List<JobEntity>> GetJobsAsync()
  {
    const string query = @$"SELECT * FROM `{TableName}` WHERE `Enabled` = 1";
    await using var conn = _connectionFactory.GetConnection();
    return (await conn.QueryAsync<JobEntity>(query)).ToList();
  }
}
