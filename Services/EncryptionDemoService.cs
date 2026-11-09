using System.Data.Common;
using AttendanceUI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AttendanceUI.Services;

/// <summary>A table that has encrypted columns, as offered on the Encryption Demo page.</summary>
public record EncryptedTableInfo(string Table, string DisplayName, IReadOnlyList<string> Columns);

/// <summary>One encrypted column of one row: what the database holds vs. what the app shows.</summary>
public record EncryptedCellSample(string Column, string StoredValue, string AppValue, bool IsEncrypted, bool IsBinary);

public record EncryptedRowSample(long Id, string Label, IReadOnlyList<EncryptedCellSample> Cells);

public interface IEncryptionDemoService
{
    IReadOnlyList<EncryptedTableInfo> GetTables();

    /// <summary>
    /// Reads the raw stored values of the encrypted columns (bypassing EF's decrypting
    /// converters) for the newest rows of <paramref name="table"/>, alongside the
    /// decrypted value the app displays. Read-only.
    /// </summary>
    Task<IReadOnlyList<EncryptedRowSample>> GetSamplesAsync(string table, int limit, CancellationToken ct = default);
}

/// <summary>
/// Backs the read-only Encryption Demo page. Which tables and columns are encrypted is
/// taken from the EF model (<see cref="SchoolDbContext.EncryptionOf"/>), so the demo always
/// matches what the app really encrypts.
/// </summary>
public class EncryptionDemoService(IDbContextFactory<SchoolDbContext> contextFactory) : IEncryptionDemoService
{
    private static readonly Dictionary<string, string> FriendlyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["students"] = "Students",
        ["teachers"] = "Teachers",
        ["leave_requests"] = "Leave requests",
        ["student_excuse_requests"] = "Excuse letters",
        ["audit_logs"] = "Audit log",
    };

    private sealed record TableMap(
        string Table,
        IProperty Key,
        IProperty? FirstName,
        IProperty? LastName,
        IReadOnlyList<(IProperty Property, SchoolDbContext.EncryptedKind Kind)> Columns);

    private IReadOnlyList<TableMap>? _maps;

    private IReadOnlyList<TableMap> Maps()
    {
        if (_maps is not null) return _maps;

        using var context = contextFactory.CreateDbContext();
        _maps = context.Model.GetEntityTypes()
            .Select(et => new
            {
                et,
                Columns = et.GetProperties()
                    .Select(p => (Property: p, Kind: SchoolDbContext.EncryptionOf(p)))
                    .Where(x => x.Kind is not null)
                    .Select(x => (x.Property, x.Kind!.Value))
                    .ToList()
            })
            .Where(x => x.Columns.Count > 0 && x.et.FindPrimaryKey() is { Properties.Count: 1 })
            .Select(x => new TableMap(
                x.et.GetTableName()!,
                x.et.FindPrimaryKey()!.Properties[0],
                x.et.FindProperty("FirstName"),
                x.et.FindProperty("LastName"),
                x.Columns))
            .OrderBy(m => m.Table)
            .ToList();
        return _maps;
    }

    public IReadOnlyList<EncryptedTableInfo> GetTables() =>
        Maps()
            .Select(m => new EncryptedTableInfo(
                m.Table,
                FriendlyNames.TryGetValue(m.Table, out var name) ? name : m.Table,
                m.Columns.Select(c => c.Property.GetColumnName()).ToList()))
            .ToList();

    private static string Quote(string identifier) => $"`{identifier}`";

    public async Task<IReadOnlyList<EncryptedRowSample>> GetSamplesAsync(string table, int limit, CancellationToken ct = default)
    {
        var map = Maps().FirstOrDefault(m => string.Equals(m.Table, table, StringComparison.OrdinalIgnoreCase))
                  ?? throw new ArgumentException($"'{table}' has no encrypted columns.", nameof(table));
        limit = Math.Clamp(limit, 1, 100);

        var select = new List<string> { Quote(map.Key.GetColumnName()) };
        if (map.FirstName is not null && map.LastName is not null)
        {
            select.Add(Quote(map.FirstName.GetColumnName()));
            select.Add(Quote(map.LastName.GetColumnName()));
        }
        var firstEncrypted = select.Count;
        select.AddRange(map.Columns.Select(c => Quote(c.Property.GetColumnName())));

        // Only rows that actually hold something in an encrypted column are interesting.
        var anyValue = string.Join(" OR ", map.Columns.Select(c => $"{Quote(c.Property.GetColumnName())} IS NOT NULL"));

        await using var context = await contextFactory.CreateDbContextAsync(ct);
        await context.Database.OpenConnectionAsync(ct);
        await using var command = context.Database.GetDbConnection().CreateCommand();
        // Identifiers come from the EF model, never from user input; LIMIT is a clamped int.
        command.CommandText =
            $"SELECT {string.Join(", ", select)} FROM {Quote(map.Table)} WHERE {anyValue} " +
            $"ORDER BY {Quote(map.Key.GetColumnName())} DESC LIMIT {limit}";

        var rows = new List<EncryptedRowSample>();
        try
        {
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = Convert.ToInt64(reader.GetValue(0));
                var label = firstEncrypted == 3
                    ? $"{reader.GetValue(2)}, {reader.GetValue(1)}"
                    : $"#{id}";

                var cells = new List<EncryptedCellSample>(map.Columns.Count);
                for (var i = 0; i < map.Columns.Count; i++)
                {
                    var (property, kind) = map.Columns[i];
                    var raw = reader.IsDBNull(firstEncrypted + i) ? null : reader.GetValue(firstEncrypted + i);
                    cells.Add(Describe(property.GetColumnName(), kind, raw));
                }

                rows.Add(new EncryptedRowSample(id, label, cells));
            }
        }
        catch (DbException) when (!ct.IsCancellationRequested)
        {
            // e.g. student_excuse_requests before its first use created it.
            return [];
        }

        return rows;
    }

    private static EncryptedCellSample Describe(string column, SchoolDbContext.EncryptedKind kind, object? raw)
    {
        switch (raw)
        {
            case null:
                return new EncryptedCellSample(column, "NULL", "(empty)", false, kind == SchoolDbContext.EncryptedKind.Bytes);

            case byte[] bytes:
            {
                var encrypted = DataEncryption.IsEncrypted(bytes);
                var stored = $"{bytes.Length:N0} bytes: {Convert.ToHexString(bytes.AsSpan(0, Math.Min(24, bytes.Length)))}{(bytes.Length > 24 ? "…" : "")}";
                string shown;
                if (!encrypted)
                {
                    shown = $"{bytes.Length:N0}-byte file";
                }
                else
                {
                    var plain = DataEncryption.DecryptBytes(bytes);
                    shown = DataEncryption.IsEncrypted(plain)
                        ? DataEncryption.Unreadable
                        : $"{plain.Length:N0}-byte file (decrypted)";
                }
                return new EncryptedCellSample(column, stored, shown, encrypted, true);
            }

            default:
            {
                var text = Convert.ToString(raw) ?? string.Empty;
                return kind == SchoolDbContext.EncryptedKind.Json
                    ? new EncryptedCellSample(column, text, DataEncryption.DecryptJson(text), DataEncryption.IsEncryptedJson(text), false)
                    : new EncryptedCellSample(column, text, DataEncryption.DecryptText(text), DataEncryption.IsEncrypted(text), false);
            }
        }
    }
}
