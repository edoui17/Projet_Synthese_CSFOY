namespace Core.Utils;

public static class LoginValidator
{
    public const int MIN_USERNAME_LENGTH = 3;

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

        if (string.IsNullOrWhiteSpace(p_password))
        {
            return (false, "Le mot de passe ne peut pas être vide.");
        }

        return (true, null);
    }
}
