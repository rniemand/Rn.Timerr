using Dapper;
using Rn.Timerr.Models.Entities;

namespace Rn.Timerr.Repos;

interface IJobsRepo
{
  Task<List<JobEntity>> GetJobsAsync(string host);
  Task<int> SetNextRunDate(JobEntity job);
  Task<JobEntity> GetJobAsync(string host, string jobName);
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
  public async Task<List<JobEntity>> GetJobsAsync(string host)
  {
    var query = @$"SELECT *
    FROM `{TableName}`
    WHERE `Enabled` = 1
      AND `Host` = '{host}'";

    await using var conn = _connectionFactory.GetConnection();
    return (await conn.QueryAsync<JobEntity>(query)).ToList();
  }

  public async Task<int> SetNextRunDate(JobEntity job)
  {
    const string query = @$"UPDATE `{TableName}`
    SET
      `NextRun` = @NextRun,
      `LastRun` = utc_timestamp(6)
    WHERE
      `JobID` = @JobID";

    await using var conn = _connectionFactory.GetConnection();
    return await conn.ExecuteAsync(query, job);
  }

  public async Task<JobEntity> GetJobAsync(string host, string jobName)
  {
    var query = @$"SELECT *
    FROM `{TableName}`
    WHERE `Enabled` = 1
      AND `Host` = '{host}'
      AND `JobName` = '{jobName}'";

    await using var conn = _connectionFactory.GetConnection();
    return await conn.QueryFirstAsync<JobEntity>(query);
  }
}
