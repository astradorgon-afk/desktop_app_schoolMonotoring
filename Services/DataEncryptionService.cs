using System.Data.Common;
using AttendanceUI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AttendanceUI.Services;

/// <summary>How many sensitive values are encrypted, and how many are still plain text.</summary>
/// <param name="Unavailable">Tables that do not exist on this database yet (they are skipped).</param>
public record EncryptionStatus(int EncryptedValues, int PlainValues, IReadOnlyList<string> Unavailable);

public interface IDataEncryptionService
{
    Task<EncryptionStatus> GetStatusAsync(CancellationToken ct = default);

    /// <summary>
    /// Encrypts values written before encryption was turned on. Already-encrypted values and
    /// the rest of each row are left untouched. Returns the number of rows updated.
    /// </summary>
    Task<int> EncryptExistingAsync(long actorUserId, string actorName, string actorRole, CancellationToken ct = default);
}

public class DataEncryptionService : IDataEncryptionService
{
    private const int BatchSize = 100;
    // Excuse letters carry their attachment, so keep those batches small.
    private const int AttachmentBatchSize = 10;

    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private readonly IAuditService _auditService;

    public DataEncryptionService(IDbContextFactory<SchoolDbContext> contextFactory, IAuditService auditService)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    /// <summary>An entity whose mapped properties include encrypted columns.</summary>
    private sealed record EncryptedTable(IEntityType EntityType, string Table, IProperty Key, IReadOnlyList<(IProperty Property, SchoolDbContext.EncryptedKind Kind)> Columns);

    private static List<EncryptedTable> EncryptedTables(SchoolDbContext context) =>
        context.Model.GetEntityTypes()
            .Select(et => new
            {
                et,
                Columns = et.GetProperties()
                    .Select(p => (Property: p, Kind: SchoolDbContext.EncryptionOf(p)))
                    .Where(x => x.Kind is not null)
                    .Select(x => (x.Property, x.Kind!.Value))
                    .ToList()
            })
            .Where(x => x.Columns.Count > 0)
            .Select(x => new EncryptedTable(x.et, x.et.GetTableName()!, x.et.FindPrimaryKey()!.Properties[0], x.Columns))
            .ToList();

    private static string Quote(string identifier) => $"`{identifier}`";

    private static string Column(IProperty property) => Quote(property.GetColumnName());

    // SQL matching values written before encryption. Empty text carries nothing, so it is ignored.
    // Backticks quote identifiers in both MySQL and SQLite.
    private static string PlainPredicate(IProperty property, SchoolDbContext.EncryptedKind kind)
    {
        var c = Column(property);
        return kind switch
        {
            SchoolDbContext.EncryptedKind.Bytes =>
                $"({c} IS NOT NULL AND SUBSTR({c}, 1, {DataEncryption.BlobMagic.Length}) <> X'{Convert.ToHexString(DataEncryption.BlobMagic)}')",
            SchoolDbContext.EncryptedKind.Json =>
                $"({c} IS NOT NULL AND CAST({c} AS CHAR) NOT LIKE '\"{DataEncryption.TextPrefix}%')",
            _ => $"({c} IS NOT NULL AND {c} <> '' AND {c} NOT LIKE '{DataEncryption.TextPrefix}%')"
        };
    }

    private static string EncryptedPredicate(IProperty property, SchoolDbContext.EncryptedKind kind)
    {
        var c = Column(property);
        return kind switch
        {
            SchoolDbContext.EncryptedKind.Bytes =>
                $"({c} IS NOT NULL AND SUBSTR({c}, 1, {DataEncryption.BlobMagic.Length}) = X'{Convert.ToHexString(DataEncryption.BlobMagic)}')",
            SchoolDbContext.EncryptedKind.Json => $"(CAST({c} AS CHAR) LIKE '\"{DataEncryption.TextPrefix}%')",
            _ => $"({c} LIKE '{DataEncryption.TextPrefix}%')"
        };
    }

    public async Task<EncryptionStatus> GetStatusAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await context.Database.OpenConnectionAsync(ct);
        var connection = context.Database.GetDbConnection();

        int encrypted = 0, plain = 0;
        var unavailable = new List<string>();
        foreach (var table in EncryptedTables(context))
        {
            var sums = string.Join(", ", table.Columns.Select(c =>
                $"SUM(CASE WHEN {EncryptedPredicate(c.Property, c.Kind)} THEN 1 ELSE 0 END), " +
                $"SUM(CASE WHEN {PlainPredicate(c.Property, c.Kind)} THEN 1 ELSE 0 END)"));

            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT {sums} FROM {Quote(table.Table)}";
            try
            {
                await using var reader = await command.ExecuteReaderAsync(ct);
                if (!await reader.ReadAsync(ct)) continue;
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    // SUM over an empty table is NULL.
                    var count = reader.IsDBNull(i) ? 0 : Convert.ToInt32(reader.GetValue(i));
                    if (i % 2 == 0) encrypted += count; else plain += count;
                }
            }
            catch (DbException) when (!ct.IsCancellationRequested)
            {
                // e.g. student_excuse_requests before its first use created it.
                unavailable.Add(table.Table);
            }
        }

        return new EncryptionStatus(encrypted, plain, unavailable);
    }

    public async Task<int> EncryptExistingAsync(long actorUserId, string actorName, string actorRole, CancellationToken ct = default)
    {
        List<EncryptedTable> tables;
        await using (var context = await _contextFactory.CreateDbContextAsync(ct))
            tables = EncryptedTables(context);

        var updated = 0;
        foreach (var table in tables)
            updated += await EncryptTableAsync(table, ct);

        await _auditService.LogAsync(actorUserId, actorName, actorRole, "DataEncryption", "Database", null,
            $"Encrypted sensitive fields in {updated} existing record{(updated == 1 ? "" : "s")}.", ct: ct);
        return updated;
    }

    /// <summary>
    /// Reads the raw stored values of rows that still have plain text, and writes back only those
    /// columns through EF, whose converters encrypt them. updated_at and every other column are
    /// left alone, and values already encrypted (even with another key) are never rewritten.
    /// </summary>
    private async Task<int> EncryptTableAsync(EncryptedTable table, CancellationToken ct)
    {
        var batchSize = table.Columns.Any(c => c.Kind == SchoolDbContext.EncryptedKind.Bytes) ? AttachmentBatchSize : BatchSize;
        var key = Column(table.Key);
        var columns = string.Join(", ", table.Columns.Select(c => Column(c.Property)));
        var anyPlain = string.Join(" OR ", table.Columns.Select(c => PlainPredicate(c.Property, c.Kind)));

        var updated = 0;
        long lastId = 0;
        while (true)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            await context.Database.OpenConnectionAsync(ct);

            var rows = new List<(long Id, object?[] Values)>();
            await using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText =
                    $"SELECT {key}, {columns} FROM {Quote(table.Table)} WHERE {key} > @last AND ({anyPlain}) ORDER BY {key} LIMIT {batchSize}";
                var last = command.CreateParameter();
                last.ParameterName = "@last";
                last.Value = lastId;
                command.Parameters.Add(last);

                try
                {
                    await using var reader = await command.ExecuteReaderAsync(ct);
                    while (await reader.ReadAsync(ct))
                    {
                        var values = new object?[table.Columns.Count];
                        for (var i = 0; i < values.Length; i++)
                            values[i] = reader.IsDBNull(i + 1) ? null : reader.GetValue(i + 1);
                        rows.Add((Convert.ToInt64(reader.GetValue(0)), values));
                    }
                }
                catch (DbException) when (!ct.IsCancellationRequested && updated == 0 && lastId == 0)
                {
                    return 0;   // table not created on this database yet
                }
            }

            if (rows.Count == 0)
                return updated;

            foreach (var (id, values) in rows)
            {
                // A stub carrying only the key and the plain values; only those columns are written.
                var entry = context.Entry(Activator.CreateInstance(table.EntityType.ClrType)!);
                entry.Property(table.Key.Name).CurrentValue = id;
                var plainColumns = new List<string>();
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    var (property, kind) = table.Columns[i];
                    object? value = values[i] switch
                    {
                        byte[] bytes when kind == SchoolDbContext.EncryptedKind.Bytes => DataEncryption.IsEncrypted(bytes) ? null : bytes,
                        string text when kind == SchoolDbContext.EncryptedKind.Json => DataEncryption.IsEncryptedJson(text) ? null : text,
                        string text when kind == SchoolDbContext.EncryptedKind.Text => DataEncryption.IsEncrypted(text) || text.Length == 0 ? null : text,
                        _ => null
                    };
                    if (value is null) continue;

                    entry.Property(property.Name).CurrentValue = value;
                    plainColumns.Add(property.Name);
                }

                entry.State = EntityState.Unchanged;
                foreach (var name in plainColumns)
                    entry.Property(name).IsModified = true;
                if (plainColumns.Count > 0)
                    updated++;
            }

            await context.SaveChangesAsync(ct);
            lastId = rows[^1].Id;
        }
    }
}
