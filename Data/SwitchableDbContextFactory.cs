using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Data;

/// <summary>
/// A <see cref="IDbContextFactory{TContext}"/> that builds a context for whichever
/// profile <see cref="DatabaseSelector.Current"/> points at. Options are cached per
/// profile, and a SQLite profile has its schema created and demo data seeded the
/// first time it is used.
/// </summary>
public sealed class SwitchableDbContextFactory : IDbContextFactory<SchoolDbContext>
{
    private readonly DatabaseSelector _selector;
    private readonly ConcurrentDictionary<string, DbContextOptions<SchoolDbContext>> _optionsCache = new();
    private readonly ConcurrentDictionary<string, bool> _initialized = new();
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public SwitchableDbContextFactory(DatabaseSelector selector)
    {
        _selector = selector;
    }

    public SchoolDbContext CreateDbContext()
    {
        var profile = _selector.Current;
        var options = OptionsFor(profile);

        // Fallback for callers that never went through EnsureInitializedAsync.
        // Blocking, so prefer warming the profile up beforehand — on a first-run
        // SQLite profile this seeds the demo dataset and takes a while.
        if (NeedsInitialization(profile))
        {
            _initLock.Wait();
            try
            {
                Initialize(profile, options);
            }
            finally
            {
                _initLock.Release();
            }
        }

        return new SchoolDbContext(options);
    }

    /// <summary>
    /// Prepares the current profile off the calling thread, creating the schema and
    /// seeding demo data if this is a SQLite profile's first use. Call this before
    /// the first query — the work is far too slow to sit on a UI thread, where
    /// Android's ANR watchdog would kill the app.
    /// </summary>
    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        var profile = _selector.Current;
        if (!NeedsInitialization(profile))
            return;

        var options = OptionsFor(profile);

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (!NeedsInitialization(profile))
                return;

            await Task.Run(() => Initialize(profile, options), cancellationToken);
        }
        finally
        {
            _initLock.Release();
        }
    }

    private DbContextOptions<SchoolDbContext> OptionsFor(ConnectionProfile profile) =>
        _optionsCache.GetOrAdd(profile.Key, _ =>
        {
            var builder = new DbContextOptionsBuilder<SchoolDbContext>();
            profile.Configure(builder);
            return builder.Options;
        });

    // Only the portable SQLite file needs its schema and demo data created.
    private bool NeedsInitialization(ConnectionProfile profile) =>
        profile.Provider == DatabaseProvider.Sqlite && !_initialized.ContainsKey(profile.Key);

    private void Initialize(ConnectionProfile profile, DbContextOptions<SchoolDbContext> options)
    {
        if (_initialized.ContainsKey(profile.Key))
            return;

        using var context = new SchoolDbContext(options);
        context.Database.EnsureCreated();
        SqliteSeeder.Seed(context);
        _initialized[profile.Key] = true;
    }
}
