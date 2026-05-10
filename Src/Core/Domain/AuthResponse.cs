namespace Core.Domain;

public class AuthResponse
{
    public string SessionToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
