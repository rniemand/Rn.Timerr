using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Rn.Timerr.Repos;

public interface IConnectionFactory
{
  MySqlConnection GetConnection();
}

public class ConnectionFactory : IConnectionFactory
{
  private readonly string _connectionString;

  public ConnectionFactory(IConfiguration configuration)
  {
    _connectionString = configuration.GetSection("ConnectionStrings:RnTimerr").Value ?? string.Empty;
  }

  public MySqlConnection GetConnection()
  {
    var conn = new MySqlConnection(_connectionString);
    conn.Open();
    return conn;
  }
}
