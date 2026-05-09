namespace Core.Domain;

public class ConfigUpsertRequest
{
    public string SessionToken { get; set; } = string.Empty;
    public PlayerConfig Config { get; set; } = null!;
}
