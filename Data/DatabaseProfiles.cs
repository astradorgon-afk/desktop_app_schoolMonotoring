namespace AttendanceUI.Data;

/// <summary>
/// The set of database connection profiles the app can use, and the logic that
/// picks the active one. Select a profile at runtime with the environment
/// variable <see cref="ProfileSelectorEnv"/> (e.g. SCHOOL_SMS_DB_PROFILE=hostinger).
/// When unset, <see cref="DefaultProfileKey"/> is used.
/// </summary>
public static class DatabaseProfiles
{
    public const string ProfileSelectorEnv = "SCHOOL_SMS_DB_PROFILE";
    public const string DefaultProfileKey = "local";

    public static readonly IReadOnlyList<ConnectionProfile> All =
    [
        new ConnectionProfile(
            "local",
            "Local MySQL",
            "Connect to the local MySQL database.",
            "SCHOOL_SMS_LOCAL_DB_CONNECTION",
            "SCHOOL_SMS_LOCAL_DB_HOST",
            "SCHOOL_SMS_LOCAL_DB_PORT",
            "SCHOOL_SMS_LOCAL_DB_NAME",
            "SCHOOL_SMS_LOCAL_DB_USER",
            "SCHOOL_SMS_LOCAL_DB_PASSWORD",
            "SCHOOL_SMS_LOCAL_DB_CHARSET",
            "SCHOOL_SMS_LOCAL_DB_SSLMODE",
            "localhost",
            3306,
            "School_sms",
            "root",
            "root",
            "utf8mb4",
            "None"),

        // NOTE: the literal host/user/password below are fallback defaults only.
        // Prefer setting the SCHOOL_SMS_HOSTINGER_DB_* environment variables so the
        // password is not carried in source control.
        new ConnectionProfile(
            "hostinger",
            "Hostinger",
            "Connect to the remote Hostinger MySQL database.",
            "SCHOOL_SMS_HOSTINGER_DB_CONNECTION",
            "SCHOOL_SMS_HOSTINGER_DB_HOST",
            "SCHOOL_SMS_HOSTINGER_DB_PORT",
            "SCHOOL_SMS_HOSTINGER_DB_NAME",
            "SCHOOL_SMS_HOSTINGER_DB_USER",
            "SCHOOL_SMS_HOSTINGER_DB_PASSWORD",
            "SCHOOL_SMS_HOSTINGER_DB_CHARSET",
            "SCHOOL_SMS_HOSTINGER_DB_SSLMODE",
            "srv1237.hstgr.io",
            3306,
            "u621755393_CSchoolManagen",
            "u621755393_cschool",
            "Dssc@2026",
            "utf8mb4",
            "Preferred"),

        ConnectionProfile.Sqlite(
            "sqlite",
            "SQLite (portable)",
            "Portable local SQLite database file. Schema is created automatically.",
            "SCHOOL_SMS_SQLITE_DB_CONNECTION",
            "SCHOOL_SMS_SQLITE_DB_PATH",
            "school_sms.db"),
    ];

    /// <summary>Resolves the active profile from the environment, falling back to the default.</summary>
    public static ConnectionProfile Resolve()
    {
        var key = Environment.GetEnvironmentVariable(ProfileSelectorEnv);
        if (string.IsNullOrWhiteSpace(key))
            key = DefaultProfileKey;

        return All.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase))
            ?? All.First(p => p.Key == DefaultProfileKey);
    }
}
