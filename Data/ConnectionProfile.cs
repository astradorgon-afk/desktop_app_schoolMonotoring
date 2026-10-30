using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;

namespace AttendanceUI.Data;

public enum DatabaseProvider
{
    MySql,
    Sqlite
}

/// <summary>
/// A named database connection profile. Every field can be overridden at runtime
/// by an environment variable (so secrets need not live in source); the literal
/// values passed to the constructor are only fallback defaults.
/// </summary>
public sealed class ConnectionProfile
{
    public string Key { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public DatabaseProvider Provider { get; }

    // Env var that supplies a full, ready-made connection string (highest priority).
    private readonly string _connectionEnv;

    // MySQL component env-var names.
    private readonly string? _hostEnv, _portEnv, _nameEnv, _userEnv, _passwordEnv, _charsetEnv, _sslModeEnv;

    // SQLite component env-var name.
    private readonly string? _dataSourceEnv;

    // MySQL default components.
    private readonly string? _defaultHost, _defaultName, _defaultUser, _defaultPassword, _defaultCharset, _defaultSslMode;
    private readonly int _defaultPort;

    // SQLite default component.
    private readonly string? _defaultDataSource;

    private ConnectionProfile(
        string key, string displayName, string description, DatabaseProvider provider,
        string connectionEnv,
        string? hostEnv, string? portEnv, string? nameEnv, string? userEnv,
        string? passwordEnv, string? charsetEnv, string? sslModeEnv, string? dataSourceEnv,
        string? defaultHost, int defaultPort, string? defaultName, string? defaultUser,
        string? defaultPassword, string? defaultCharset, string? defaultSslMode, string? defaultDataSource)
    {
        Key = key;
        DisplayName = displayName;
        Description = description;
        Provider = provider;
        _connectionEnv = connectionEnv;
        _hostEnv = hostEnv;
        _portEnv = portEnv;
        _nameEnv = nameEnv;
        _userEnv = userEnv;
        _passwordEnv = passwordEnv;
        _charsetEnv = charsetEnv;
        _sslModeEnv = sslModeEnv;
        _dataSourceEnv = dataSourceEnv;
        _defaultHost = defaultHost;
        _defaultPort = defaultPort;
        _defaultName = defaultName;
        _defaultUser = defaultUser;
        _defaultPassword = defaultPassword;
        _defaultCharset = defaultCharset;
        _defaultSslMode = defaultSslMode;
        _defaultDataSource = defaultDataSource;
    }

    /// <summary>Creates a MySQL connection profile.</summary>
    public ConnectionProfile(
        string key, string displayName, string description,
        string connectionEnv, string hostEnv, string portEnv, string nameEnv,
        string userEnv, string passwordEnv, string charsetEnv, string sslModeEnv,
        string defaultHost, int defaultPort, string defaultName, string defaultUser,
        string defaultPassword, string defaultCharset, string defaultSslMode)
        : this(key, displayName, description, DatabaseProvider.MySql,
            connectionEnv, hostEnv, portEnv, nameEnv, userEnv, passwordEnv, charsetEnv, sslModeEnv,
            dataSourceEnv: null,
            defaultHost, defaultPort, defaultName, defaultUser, defaultPassword, defaultCharset, defaultSslMode,
            defaultDataSource: null)
    {
    }

    /// <summary>Creates a SQLite (single file, portable) connection profile.</summary>
    public static ConnectionProfile Sqlite(
        string key, string displayName, string description,
        string connectionEnv, string dataSourceEnv, string defaultDataSource) =>
        new(key, displayName, description, DatabaseProvider.Sqlite,
            connectionEnv,
            hostEnv: null, portEnv: null, nameEnv: null, userEnv: null,
            passwordEnv: null, charsetEnv: null, sslModeEnv: null, dataSourceEnv,
            defaultHost: null, defaultPort: 0, defaultName: null, defaultUser: null,
            defaultPassword: null, defaultCharset: null, defaultSslMode: null, defaultDataSource);

    /// <summary>Applies this profile's provider and connection string to the options builder.</summary>
    public void Configure(DbContextOptionsBuilder options)
    {
        var connectionString = BuildConnectionString();

        if (Provider == DatabaseProvider.Sqlite)
            options.UseSqlite(connectionString);
        else
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }

    public string BuildConnectionString() =>
        Provider == DatabaseProvider.Sqlite ? BuildSqlite() : BuildMySql();

    private string BuildMySql()
    {
        var full = Env(_connectionEnv);
        if (full is not null)
            return full;

        var host = Env(_hostEnv) ?? _defaultHost;
        var port = int.TryParse(Env(_portEnv), out var p) ? p : _defaultPort;
        var name = Env(_nameEnv) ?? _defaultName;
        var user = Env(_userEnv) ?? _defaultUser;
        var password = Env(_passwordEnv) ?? _defaultPassword;
        var charset = Env(_charsetEnv) ?? _defaultCharset;
        var sslMode = Env(_sslModeEnv) ?? _defaultSslMode;

        var connectionString =
            $"Server={host};Port={port};Database={name};User Id={user};" +
            $"Password={password};CharSet={charset};SslMode={sslMode};";

        // MySQL 8 authenticates with caching_sha2_password by default. On an
        // unencrypted connection the client cannot complete the first handshake
        // unless it may fetch the server's RSA public key. Only added when TLS is
        // off — with TLS the handshake is already protected and this would
        // needlessly widen the trust boundary. Fetching the key over an
        // untrusted network is MITM-able, so keep SslMode=None for local hosts.
        if (IsTlsDisabled(sslMode))
            connectionString += "AllowPublicKeyRetrieval=True;";

        return connectionString;
    }

    private string BuildSqlite()
    {
        var full = Env(_connectionEnv);
        if (full is not null)
            return full;

        var dataSource = Env(_dataSourceEnv) ?? _defaultDataSource ?? "school_sms.db";

        // A bare filename is placed in the app's writable data directory so the
        // file is portable and does not depend on the working directory.
        if (!Path.IsPathRooted(dataSource))
            dataSource = Path.Combine(FileSystem.AppDataDirectory, dataSource);

        return $"Data Source={dataSource}";
    }

    private static bool IsTlsDisabled(string? sslMode) =>
        string.Equals(sslMode, "None", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(sslMode, "Disabled", StringComparison.OrdinalIgnoreCase);

    private static string? Env(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
