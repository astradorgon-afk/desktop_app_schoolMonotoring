namespace AttendanceUI.Services;

/// <summary>
/// Attendance session dates are stored as UTC (<c>DateTime.UtcNow</c>), but people filter and
/// read them as local calendar days. Every date filter must convert its local-day bounds to
/// UTC, and every displayed or grouped date must be converted back, or sessions near midnight
/// land on the wrong day (testing report defect D3).
/// </summary>
public static class SessionDates
{
    /// <summary>The UTC instant at which local calendar day <paramref name="day"/> begins.</summary>
    public static DateTime LocalDayStartUtc(DateOnly day) =>
        DateTime.SpecifyKind(day.ToDateTime(TimeOnly.MinValue), DateTimeKind.Local).ToUniversalTime();

    /// <summary>Converts a stored (UTC) session timestamp to local time.</summary>
    public static DateTime ToLocal(DateTime storedUtc) =>
        DateTime.SpecifyKind(storedUtc, DateTimeKind.Utc).ToLocalTime();

    /// <summary>The local calendar day of a stored (UTC) session timestamp.</summary>
    public static DateOnly LocalDate(DateTime storedUtc) => DateOnly.FromDateTime(ToLocal(storedUtc));
}
