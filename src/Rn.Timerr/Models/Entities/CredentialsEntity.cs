namespace Rn.Timerr.Models.Entities;

class CredentialsEntity
{
  public string Host { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Credentials { get; set; } = string.Empty;
  public bool Deleted { get; set; }
}
