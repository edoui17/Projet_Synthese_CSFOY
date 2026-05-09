namespace Core.Domain;

public class StatsUpsertRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public PlayerStats Stats { get; set; } = null!;
}
