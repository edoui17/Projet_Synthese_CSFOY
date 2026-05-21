using Godot;

namespace IslandSurvivor.Utils;

public static class SessionProvider
{
    private const string SESSION_PATH = "user://session.cfg";
    private const string SECTION = "Auth";
    private const string TOKEN_KEY = "token";

    public static string? GetStoredToken()
    {
        ConfigFile config = new ConfigFile();
        Error err = config.Load(SESSION_PATH);

        if (err != Error.Ok)
        {
            return null;
        }

        return config.GetValue(SECTION, TOKEN_KEY, new Variant()).AsString();
    }

    public static void StoreToken(string? p_token)
    {
        ConfigFile config = new ConfigFile();

        // Load existing to preserve other settings if any
        config.Load(SESSION_PATH);

        if (string.IsNullOrEmpty(p_token))
        {
            if (config.HasSectionKey(SECTION, TOKEN_KEY))
            {
                config.EraseSectionKey(SECTION, TOKEN_KEY);
            }
        }
        else
        {
            config.SetValue(SECTION, TOKEN_KEY, p_token);
        }

        Error err = config.Save(SESSION_PATH);
        if (err != Error.Ok)
        {
            GD.PrintErr($"[SessionProvider] Failed to save session token: {err}");
        }
    }

    public static void ClearToken()
    {
        StoreToken(null);
    }
}
