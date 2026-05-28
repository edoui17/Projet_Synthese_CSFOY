namespace Core.Utils;

public static class LoginValidator
{
    public const int MIN_USERNAME_LENGTH = 3;

    // Security: Enforce explicit max limits to prevent buffer overflow or DoS when syncing credentials
    public const int MAX_USERNAME_LENGTH = 50;
    public const int MAX_PASSWORD_LENGTH = 100;

    public static (bool IsValid, string? ErrorMessage) Validate(string p_username, string p_password)
    {
        if (string.IsNullOrWhiteSpace(p_username))
        {
            return (false, "L'identifiant ne peut pas être vide.");
        }

        if (p_username.Length < MIN_USERNAME_LENGTH)
        {
            return (false, $"L'identifiant doit contenir au moins {MIN_USERNAME_LENGTH} caractères.");
        }

        if (p_username.Length > MAX_USERNAME_LENGTH)
        {
            return (false, $"L'identifiant ne peut pas dépasser {MAX_USERNAME_LENGTH} caractères.");
        }

        if (string.IsNullOrWhiteSpace(p_password))
        {
            return (false, "Le mot de passe ne peut pas être vide.");
        }

        if (p_password.Length > MAX_PASSWORD_LENGTH)
        {
            return (false, $"Le mot de passe ne peut pas dépasser {MAX_PASSWORD_LENGTH} caractères.");
        }

        return (true, null);
    }
}
