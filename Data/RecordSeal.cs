using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AttendanceUI.Data.Entities;

namespace AttendanceUI.Data;

public enum SealState
{
    /// <summary>The seal matches the record.</summary>
    Intact,
    /// <summary>The record has a seal, but its values no longer match it.</summary>
    Altered,
    /// <summary>Written before sealing existed, or by another system: nothing to check against.</summary>
    Unsealed
}

/// <summary>
/// A keyed HMAC-SHA256 seal over the facts of an attendance record (session, student, status,
/// who marked it), stored in <c>attendance_records.hash</c>. <see cref="SchoolDbContext"/>
/// refreshes it on every save, so a seal that no longer matches means the row was changed
/// directly in the database. Unlike a plain hash, it cannot be recalculated without the key.
///
/// The key is derived from the data encryption key, so every device sharing a database
/// agrees on it, and a seal made with a different key reads as <see cref="SealState.Altered"/>.
/// </summary>
public static class RecordSeal
{
    /// <summary>Marks a seal: "hmac:v1:" + lowercase hex of the 32-byte HMAC.</summary>
    public const string Prefix = "hmac:v1:";

    private const string KeyPurpose = "EduAttend attendance record seal v1";

    /// <summary>The exact text that is signed. Shown on the demo page.</summary>
    public static string Message(long sessionId, long studentId, string status, long? markedByUserId) =>
        string.Create(CultureInfo.InvariantCulture, $"{sessionId}|{studentId}|{status}|{markedByUserId}");

    public static string Compute(long sessionId, long studentId, string status, long? markedByUserId)
    {
        var message = Encoding.UTF8.GetBytes(Message(sessionId, studentId, status, markedByUserId));
        return Prefix + Convert.ToHexStringLower(HMACSHA256.HashData(DataEncryption.DeriveKey(KeyPurpose), message));
    }

    public static string Compute(AttendanceRecord record) =>
        Compute(record.AttendanceSessionId, record.StudentId, record.Status, record.MarkedByUserId);

    public static bool IsSealed(string? stored) =>
        stored is not null && stored.StartsWith(Prefix, StringComparison.Ordinal);

    public static SealState Check(long sessionId, long studentId, string status, long? markedByUserId, string? stored)
    {
        if (!IsSealed(stored))
            return SealState.Unsealed;

        var expected = Encoding.ASCII.GetBytes(Compute(sessionId, studentId, status, markedByUserId));
        return CryptographicOperations.FixedTimeEquals(expected, Encoding.UTF8.GetBytes(stored!))
            ? SealState.Intact
            : SealState.Altered;
    }
}
