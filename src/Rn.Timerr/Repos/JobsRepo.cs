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

  public JobsRepo(IConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<List<JobEntity>> GetJobsAsync()
  {
    const string query = @"SELECT * FROM `Jobs` WHERE `Enabled` = 1";
    await using var conn = _connectionFactory.GetConnection();
    return (await conn.QueryAsync<JobEntity>(query)).ToList();
  }
}
