using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Data;

/// <summary>
/// Creates tables added after a database was first set up. The SQLite demo file is only
/// built by EnsureCreated on first run and MySQL is migrated by hand, so a table introduced
/// later may be missing; this adds it on first use when the database user is allowed to.
/// </summary>
public static class SchemaGuard
{
    // Keyed by table + connection string, so each database profile is checked once per run.
    private static readonly ConcurrentDictionary<string, bool> Ready = new();

    /// <summary>Returns true when <paramref name="table"/> exists (creating it if needed).</summary>
    public static async Task<bool> EnsureTableAsync(SchoolDbContext context, string table, string mySqlDdl, string sqliteDdl,
        Func<SchoolDbContext, CancellationToken, Task> probe, CancellationToken ct)
    {
        var key = table + "|" + (context.Database.GetConnectionString() ?? string.Empty);
        if (Ready.ContainsKey(key)) return true;

        try
        {
            var isSqlite = context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            await context.Database.ExecuteSqlRawAsync(isSqlite ? sqliteDdl : mySqlDdl, ct);
            Ready[key] = true;
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Most likely the database user lacks CREATE rights; the table may still have
            // been created by hand from the migration script.
            try
            {
                await probe(context, ct);
                Ready[key] = true;
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }
    }
}
