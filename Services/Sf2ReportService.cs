using System.Globalization;
using System.Text;
using AttendanceUI.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public interface ISf2ReportService
{
    /// <summary>
    /// Builds a DepEd School Form 2 (Daily Attendance Report of Learners) style CSV for one
    /// section and month. Days with no class for the section are left out.
    /// </summary>
    Task<ExportFile> BuildAsync(long sectionId, int year, int month, CancellationToken ct = default);
}

public class Sf2ReportService : ISf2ReportService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public Sf2ReportService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task<ExportFile> BuildAsync(long sectionId, int year, int month, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var section = await context.Sections.AsNoTracking()
            .Where(s => s.Id == sectionId)
            .Select(s => new { s.Id, s.Name, s.SchoolYearId, s.GradeLevelId })
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Section not found.");
        var grade = await context.GradeLevels.Where(g => g.Id == section.GradeLevelId).Select(g => g.Name).FirstOrDefaultAsync(ct);
        var schoolYear = await context.SchoolYears.Where(y => y.Id == section.SchoolYearId).Select(y => y.Name).FirstOrDefaultAsync(ct);

        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Local);
        var fromUtc = monthStart.ToUniversalTime();
        var toUtc = monthStart.AddMonths(1).ToUniversalTime();

        var learners = await context.Enrollments.AsNoTracking()
            .Where(e => e.SectionId == sectionId && e.SchoolYearId == section.SchoolYearId && e.Status == "ACTIVE")
            .Join(context.Students, e => e.StudentId, s => s.Id, (e, s) => new { s.Id, s.Lrn, s.LastName, s.FirstName, s.MiddleName, s.Sex })
            .ToListAsync(ct);

        var records = await context.AttendanceRecords.AsNoTracking()
            .Where(r => r.Session!.ClassOffering!.SectionId == sectionId
                && r.Session.SessionDate >= fromUtc
                && r.Session.SessionDate < toUtc)
            .Select(r => new { r.StudentId, r.Status, r.Session!.SessionDate })
            .ToListAsync(ct);

        // A school day is any day this section held at least one class.
        var days = records
            .Select(r => DateOnly.FromDateTime(r.SessionDate.ToLocalTime()))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var dayStatus = records
            .GroupBy(r => (r.StudentId, Day: DateOnly.FromDateTime(r.SessionDate.ToLocalTime())))
            .ToDictionary(g => g.Key, g => DailyStatus(g.Select(r => r.Status)));

        var sb = new StringBuilder();
        WriteRow(sb, "School Form 2 (SF2) Daily Attendance Report of Learners");
        WriteRow(sb, "School Year", schoolYear, "Month", monthStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture));
        WriteRow(sb, "Grade Level", grade, "Section", section.Name);
        WriteRow(sb, "Legend", "blank = present; x = absent; L = late (tardy); E = excused");
        WriteRow(sb);

        var header = new List<string?> { "No.", "LRN", "Learner's Name (Last, First, Middle)" };
        header.AddRange(days.Select(d => d.ToString("d ddd", CultureInfo.InvariantCulture)));
        header.AddRange(["Total Absent", "Total Tardy", "Total Excused"]);
        WriteRow(sb, [.. header]);

        var dailyAbsent = new int[days.Count];
        var dailyPresent = new int[days.Count];

        foreach (var (label, isGroup) in new[] { ("MALE", (Func<string?, bool>)IsMale), ("FEMALE", IsFemale), ("UNSPECIFIED", s => !IsMale(s) && !IsFemale(s)) })
        {
            var group = learners.Where(l => isGroup(l.Sex)).OrderBy(l => l.LastName).ThenBy(l => l.FirstName).ToList();
            if (group.Count == 0) continue;

            WriteRow(sb, label);
            var n = 0;
            foreach (var l in group)
            {
                var row = new List<string?> { (++n).ToString(), l.Lrn, $"{l.LastName}, {l.FirstName}{(string.IsNullOrWhiteSpace(l.MiddleName) ? "" : " " + l.MiddleName)}" };
                int absent = 0, tardy = 0, excused = 0;
                for (var i = 0; i < days.Count; i++)
                {
                    var status = dayStatus.GetValueOrDefault((l.Id, days[i]));
                    row.Add(status switch { "ABSENT" => "x", "LATE" => "L", "EXCUSED" => "E", _ => "" });
                    switch (status)
                    {
                        case "ABSENT": absent++; dailyAbsent[i]++; break;
                        case "LATE": tardy++; dailyPresent[i]++; break;
                        case "EXCUSED": excused++; break;
                        case "PRESENT": dailyPresent[i]++; break;
                    }
                }
                row.AddRange([absent.ToString(), tardy.ToString(), excused.ToString()]);
                WriteRow(sb, [.. row]);
            }
        }

        WriteRow(sb);
        var presentRow = new List<string?> { "", "", "Total present per day" };
        presentRow.AddRange(dailyPresent.Select(c => c.ToString()));
        WriteRow(sb, [.. presentRow]);
        var absentRow = new List<string?> { "", "", "Total absent per day" };
        absentRow.AddRange(dailyAbsent.Select(c => c.ToString()));
        WriteRow(sb, [.. absentRow]);

        if (days.Count == 0)
            WriteRow(sb, "", "", "No classes were recorded for this section in this month.");

        var fileName = $"SF2-{Slug(section.Name)}-{year:0000}-{month:00}.csv";
        var content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return new ExportFile(fileName, content);
    }

    /// <summary>
    /// SF2 is a daily record but this school takes attendance per class. A learner counts as
    /// present if present in any class that day, otherwise late, then excused, then absent.
    /// </summary>
    internal static string? DailyStatus(IEnumerable<string> statuses)
    {
        var set = statuses.ToHashSet();
        if (set.Contains("PRESENT")) return "PRESENT";
        if (set.Contains("LATE")) return "LATE";
        if (set.Contains("EXCUSED")) return "EXCUSED";
        if (set.Contains("ABSENT")) return "ABSENT";
        return null;
    }

    private static bool IsMale(string? sex) => sex is not null && sex.Trim().StartsWith("M", StringComparison.OrdinalIgnoreCase);
    private static bool IsFemale(string? sex) => sex is not null && sex.Trim().StartsWith("F", StringComparison.OrdinalIgnoreCase);

    private static string Slug(string value)
    {
        var chars = value.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        return new string(chars).Trim('-');
    }

    private static void WriteRow(StringBuilder sb, params string?[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(Escape(values[i]));
        }
        sb.Append("\r\n");
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        // Keep spreadsheet apps from treating names as formulas.
        if (value[0] is '=' or '+' or '-' or '@') value = "'" + value;
        var needsQuoting = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        return needsQuoting ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}
