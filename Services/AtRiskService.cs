using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using AttendanceUI.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class AtRiskStudent
{
    public long StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Lrn { get; init; } = string.Empty;
    public string? SectionName { get; init; }
    public string? GuardianName { get; init; }
    public string? GuardianContact { get; init; }
    public int Sessions { get; init; }
    public int Present { get; init; }
    public int Late { get; init; }
    public int Absent { get; init; }
    public int Excused { get; init; }
    public int ConsecutiveAbsences { get; init; }
    public double AttendanceRate { get; init; }
    public DateTime? LastAbsence { get; init; }
    public List<string> Reasons { get; init; } = [];

    /// <summary>Higher is more urgent; used for ordering.</summary>
    public int Severity => ConsecutiveAbsences * 3 + Absent * 2 + (int)Math.Max(0, 80 - AttendanceRate);
}

public interface IAtRiskService
{
    /// <summary>
    /// Students with worrying attendance this school year. Admins see everyone; teachers see
    /// students in the sections they teach or advise.
    /// </summary>
    Task<List<AtRiskStudent>> GetAtRiskAsync(UserSession viewer, CancellationToken ct = default);
}

public class AtRiskService : IAtRiskService
{
    public const int AbsenceThreshold = 3;
    public const int StreakThreshold = 3;
    public const double RateThreshold = 80;
    public const int MinSessionsForRate = 5;

    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public AtRiskService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task<List<AtRiskStudent>> GetAtRiskAsync(UserSession viewer, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var activeYear = await context.SchoolYears
            .Where(y => y.Status == "ACTIVE")
            .Select(y => y.Id)
            .FirstOrDefaultAsync(ct);
        if (activeYear == 0) return [];

        List<long>? scope = null;
        if (viewer.Role == UserRole.TEACHER)
            scope = await TeacherScope.GetStudentIdsAsync(context, viewer.UserId, ct);
        else if (viewer.Role != UserRole.SUPERADMIN)
            return [];

        var recordsQuery = context.AttendanceRecords.AsNoTracking()
            .Where(r => r.Session!.ClassOffering!.SchoolYearId == activeYear);
        if (scope is not null)
            recordsQuery = recordsQuery.Where(r => scope.Contains(r.StudentId));

        var records = await recordsQuery
            .Select(r => new { r.StudentId, r.Status, r.Session!.SessionDate })
            .ToListAsync(ct);

        var flagged = new List<(long StudentId, AtRiskStudent Partial)>();
        foreach (var group in records.GroupBy(r => r.StudentId))
        {
            var ordered = group.OrderBy(r => r.SessionDate).ToList();
            var present = ordered.Count(r => r.Status == "PRESENT");
            var late = ordered.Count(r => r.Status == "LATE");
            var absent = ordered.Count(r => r.Status == "ABSENT");
            var excused = ordered.Count(r => r.Status == "EXCUSED");

            // Excused absences don't count against the student.
            var counted = present + late + absent;
            var rate = counted > 0 ? Math.Round((double)(present + late) / counted * 100, 1) : 100;

            var streak = 0;
            for (var i = ordered.Count - 1; i >= 0 && ordered[i].Status is "ABSENT" or "EXCUSED"; i--)
                if (ordered[i].Status == "ABSENT") streak++;

            var reasons = new List<string>();
            if (streak >= StreakThreshold) reasons.Add($"{streak} absences in a row");
            if (absent >= AbsenceThreshold) reasons.Add($"{absent} absences this year");
            if (counted >= MinSessionsForRate && rate < RateThreshold) reasons.Add($"{rate:0.#}% attendance");
            if (reasons.Count == 0) continue;

            flagged.Add((group.Key, new AtRiskStudent
            {
                StudentId = group.Key,
                Sessions = ordered.Count,
                Present = present,
                Late = late,
                Absent = absent,
                Excused = excused,
                ConsecutiveAbsences = streak,
                AttendanceRate = rate,
                LastAbsence = ordered.LastOrDefault(r => r.Status == "ABSENT")?.SessionDate,
                Reasons = reasons
            }));
        }

        if (flagged.Count == 0) return [];

        var ids = flagged.Select(f => f.StudentId).ToList();
        var students = await context.Students.AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.FirstName, s.LastName, s.Lrn, s.GuardianName, s.GuardianContact, s.Status })
            .ToDictionaryAsync(s => s.Id, ct);
        var sections = await context.Enrollments.AsNoTracking()
            .Where(e => ids.Contains(e.StudentId) && e.SchoolYearId == activeYear && e.Status == "ACTIVE")
            .Join(context.Sections, e => e.SectionId, s => s.Id, (e, s) => new { e.StudentId, s.Name })
            .ToListAsync(ct);
        var sectionByStudent = sections.GroupBy(s => s.StudentId).ToDictionary(g => g.Key, g => g.First().Name);

        return flagged
            .Where(f => students.TryGetValue(f.StudentId, out var s) && s.Status == "ACTIVE")
            .Select(f =>
            {
                var s = students[f.StudentId];
                var p = f.Partial;
                return new AtRiskStudent
                {
                    StudentId = p.StudentId,
                    FullName = $"{s.LastName}, {s.FirstName}",
                    Lrn = s.Lrn,
                    SectionName = sectionByStudent.GetValueOrDefault(p.StudentId),
                    GuardianName = s.GuardianName,
                    GuardianContact = s.GuardianContact,
                    Sessions = p.Sessions,
                    Present = p.Present,
                    Late = p.Late,
                    Absent = p.Absent,
                    Excused = p.Excused,
                    ConsecutiveAbsences = p.ConsecutiveAbsences,
                    AttendanceRate = p.AttendanceRate,
                    LastAbsence = p.LastAbsence,
                    Reasons = p.Reasons
                };
            })
            .OrderByDescending(s => s.Severity)
            .ThenBy(s => s.FullName)
            .ToList();
    }
}
