namespace Rn.Timerr.Models;

class ValidationOutcome
{
  public bool Success { get; set; } = true;
  public string ValidationError { get; set; } = string.Empty;

  public ValidationOutcome WithError(string error)
  {
    Success = false;
    ValidationError = error;
    return this;
  }
}
