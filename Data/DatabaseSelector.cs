namespace AttendanceUI.Data;

/// <summary>
/// Holds the database connection profile the app is currently using. Unlike the
/// startup-only resolution, this can be changed at runtime (e.g. chosen on the
/// login page). All database access goes through <see cref="SwitchableDbContextFactory"/>,
/// which reads <see cref="Current"/> on every context creation.
/// </summary>
public sealed class DatabaseSelector
{
    private ConnectionProfile _current = DatabaseProfiles.Resolve();

    public ConnectionProfile Current => _current;

    public IReadOnlyList<ConnectionProfile> Profiles => DatabaseProfiles.All;

    public void Select(string key)
    {
        _current = DatabaseProfiles.All
            .FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Unknown database profile '{key}'.", nameof(key));
    }
}
