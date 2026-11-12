using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Data;

/// <summary>
/// Demo activity for the portable SQLite profile, on top of the school that
/// <see cref="SqliteSeeder"/> creates: every class meeting of the last six weeks as an ended
/// session with a record per student, plus leave requests, excuse letters and audit entries.
/// Dates are relative to the day of seeding, so charts for "this week" and "this month" are
/// filled whenever the demo file is created.
///
/// The data is shaped, not uniform, so the screens have something to show: Mondays and
/// Fridays dip, one day has a spike in absences, a handful of students are chronically
/// absent or on an absence streak (At-Risk), and approved excuses turn absences into EXCUSED.
/// </summary>
public static class SqliteActivitySeeder
{
    private const int Weeks = 6;
    private const string AbsentReason = "Not scanned before the session ended";

    private static readonly string[] ExcuseCategories =
        ["Sick", "Medical appointment", "Family emergency", "Bereavement", "School activity"];

    private static readonly string[] ExcuseReasons =
    [
        "Fever and cough; advised by the doctor to rest at home.",
        "Scheduled dental check-up at the health center.",
        "Had to travel to the province for a family emergency.",
        "Attending the wake of a grandparent.",
        "Representing the school at the division press conference.",
    ];

    private static readonly (string Category, string Reason)[] LeaveReasons =
    [
        ("Sick Leave", "Flu symptoms; will submit a medical certificate on return."),
        ("Business Leave", "Attending the regional teachers' training seminar."),
        ("Personal Leave", "Processing documents at the city hall."),
        ("Emergency Leave", "Immediate family member was hospitalized."),
        ("Sick Leave", "Follow-up check-up after a minor procedure."),
        ("Other", "Serving as a resource speaker at a partner school."),
    ];

    public static void Seed(SchoolDbContext context)
    {
        var rng = new Random(20261113);
        var nowLocal = DateTime.Now;
        var today = DateOnly.FromDateTime(nowLocal);

        var admin = context.Users.AsNoTracking().FirstOrDefault(u => u.Role == UserRole.SUPERADMIN);
        var offerings = context.ClassOfferings.AsNoTracking()
            .Select(o => new
            {
                o.Id, o.SectionId, TeacherId = o.TeacherId ?? 0,
                Section = o.Section!.Name,
                Subject = o.Subject!.Title,
            })
            .ToList();
        var schedules = context.ClassSchedules.AsNoTracking().ToList();
        if (admin is null || offerings.Count == 0 || schedules.Count == 0)
            return;

        var offeringById = offerings.ToDictionary(o => o.Id);
        var teachers = context.Teachers.AsNoTracking()
            .Select(t => new { t.Id, t.UserId, t.FirstName, t.LastName })
            .ToList();
        var teacherById = teachers.ToDictionary(t => t.Id);
        var students = context.Students.AsNoTracking()
            .Select(s => new { s.Id, s.UserId, s.FirstName, s.LastName })
            .ToList();
        var studentsBySection = context.Enrollments.AsNoTracking()
            .Select(e => new { e.SectionId, e.StudentId })
            .ToList()
            .GroupBy(e => e.SectionId)
            .ToDictionary(g => g.Key, g => g.Select(e => e.StudentId).ToList());

        // School days: the weekdays of the last six weeks, up to and including today.
        var days = Enumerable.Range(0, Weeks * 7)
            .Select(i => today.AddDays(-i))
            .Where(d => d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            .OrderBy(d => d)
            .ToList();
        if (days.Count < 10)
            return;

        var audit = new List<AuditLogEntry>();
        void Log(DateTime whenUtc, long actorId, string actorName, string role, string action, string entity, long? entityId, string summary) =>
            audit.Add(new AuditLogEntry
            {
                ActorId = actorId, ActorName = actorName, ActorRole = role, Action = action,
                EntityType = entity, EntityId = entityId, Summary = summary, CreatedAt = whenUtc,
            });

        // ── Excuse letters ──
        // Approved ones are remembered per student so the matching absences become EXCUSED.
        var excusedDays = new Dictionary<long, List<(DateOnly From, DateOnly To, string Reason)>>();
        var excuses = new List<StudentExcuseRequest>();
        var excuseStudents = students.OrderBy(_ => rng.Next()).Take(16).ToList();
        for (var i = 0; i < excuseStudents.Count; i++)
        {
            var student = excuseStudents[i];
            var status = i < 9 ? "APPROVED" : i < 13 ? "PENDING" : "REJECTED";
            // Pending letters are recent; the rest are spread over the earlier weeks.
            var start = status == "PENDING" ? days[^rng.Next(1, 4)] : days[rng.Next(2, days.Count - 6)];
            var end = start.AddDays(rng.Next(0, 3));
            var kind = rng.Next(ExcuseCategories.Length);
            var filed = ToUtc(start.AddDays(-1), new TimeOnly(18, rng.Next(0, 60)));

            excuses.Add(new StudentExcuseRequest
            {
                StudentId = student.Id,
                Category = ExcuseCategories[kind],
                Reason = ExcuseReasons[kind],
                StartDate = start,
                EndDate = end,
                Status = status,
                ReviewedByUserId = status == "PENDING" ? null : admin.Id,
                ReviewNotes = status == "REJECTED" ? "Please attach a supporting document and file again." : null,
                CreatedAt = filed,
                UpdatedAt = status == "PENDING" ? filed : filed.AddHours(14),
            });

            if (status == "APPROVED")
            {
                if (!excusedDays.TryGetValue(student.Id, out var list))
                    excusedDays[student.Id] = list = [];
                list.Add((start, end, $"Excused: {ExcuseCategories[kind]}"));
            }

            Log(filed, student.UserId, $"{student.FirstName} {student.LastName}", "STUDENT", "ExcuseSubmit", "StudentExcuse", null,
                $"{student.LastName}, {student.FirstName} filed an excuse: {ExcuseCategories[kind]} ({start} - {end})");
            if (status != "PENDING")
                Log(filed.AddHours(14), admin.Id, admin.Name, "SUPERADMIN", status == "APPROVED" ? "ExcuseApprove" : "ExcuseReject", "StudentExcuse", null,
                    $"Excuse request from {student.LastName}, {student.FirstName} {(status == "APPROVED" ? "approved" : "rejected")}.");
        }

        // ── Leave requests ──
        var leaves = new List<LeaveRequest>();
        var leaveTeachers = teachers.OrderBy(_ => rng.Next()).Take(14).ToList();
        for (var i = 0; i < leaveTeachers.Count; i++)
        {
            var teacher = leaveTeachers[i];
            var status = i < 6 ? "APPROVED" : i < 11 ? "PENDING" : "REJECTED";
            // Pending requests are for the coming days; reviewed ones are in the past.
            var start = status == "PENDING" ? today.AddDays(rng.Next(1, 10)) : days[rng.Next(0, days.Count - 3)];
            var end = start.AddDays(rng.Next(0, 3));
            var (category, reason) = LeaveReasons[rng.Next(LeaveReasons.Length)];
            var filed = ToUtc(status == "PENDING" ? today.AddDays(-rng.Next(0, 3)) : start.AddDays(-3), new TimeOnly(rng.Next(8, 17), rng.Next(0, 60)));
            if (filed > DateTime.UtcNow) filed = DateTime.UtcNow.AddMinutes(-rng.Next(5, 120));

            leaves.Add(new LeaveRequest
            {
                TeacherId = teacher.Id,
                Category = category,
                Reason = reason,
                StartDate = start,
                EndDate = end,
                Status = status,
                ReviewedByUserId = status == "PENDING" ? null : admin.Id,
                ReviewNotes = status == "REJECTED" ? "No substitute teacher is available on these dates." : null,
                CreatedAt = filed,
                UpdatedAt = status == "PENDING" ? filed : filed.AddHours(20),
            });

            Log(filed, teacher.Id, $"{teacher.LastName}, {teacher.FirstName}", "TEACHER", "LeaveSubmit", "LeaveRequest", null,
                $"Leave request submitted: {category} ({start} - {end})");
            if (status != "PENDING")
                Log(filed.AddHours(20), admin.Id, admin.Name, "SUPERADMIN", status == "APPROVED" ? "LeaveApprove" : "LeaveReject", "LeaveRequest", null,
                    $"Leave request from {teacher.LastName}, {teacher.FirstName} {(status == "APPROVED" ? "approved" : "rejected")}.");
        }

        // ── How each student tends to attend ──
        var absenceChance = new Dictionary<long, double>();
        var onStreak = new HashSet<long>();
        foreach (var student in students)
        {
            var roll = rng.NextDouble();
            // A student has about 60 class meetings in six weeks and is flagged At-Risk from
            // three absences, so "reliable" has to mean well under one absence in sixty.
            absenceChance[student.Id] = roll < 0.03 ? 0.25 + rng.NextDouble() * 0.2   // chronically absent
                : roll < 0.10 ? 0.06 + rng.NextDouble() * 0.05                         // patchy
                : 0.003 + rng.NextDouble() * 0.012;                                    // reliable
            if (roll > 0.98) onStreak.Add(student.Id);                                 // stopped coming this week
        }
        var streakFrom = days[^2];
        var badDay = days[^rng.Next(6, 12)];

        // ── Sessions: one per scheduled meeting on each school day ──
        var sessions = new List<(AttendanceSession Session, long TeacherUserId, DateOnly Day, DateTime StartUtc)>();
        foreach (var day in days)
        {
            foreach (var schedule in schedules.Where(s => s.DayOfWeek == (byte)day.DayOfWeek))
            {
                if (!offeringById.TryGetValue(schedule.ClassOfferingId, out var offering)) continue;
                // Classes still running or yet to start today are left for a live demo.
                if (day == today && schedule.EndTime > TimeOnly.FromDateTime(nowLocal)) continue;

                var startUtc = ToUtc(day, schedule.StartTime).AddMinutes(rng.Next(0, 4));
                var endUtc = ToUtc(day, schedule.EndTime);
                var session = new AttendanceSession
                {
                    ClassOfferingId = offering.Id,
                    SessionDate = startUtc,
                    Status = "ENDED",
                    RoomId = schedule.RoomId,
                    SectionName = offering.Section,
                    SubjectName = offering.Subject,
                    CreatedAt = startUtc,
                    UpdatedAt = endUtc,
                };
                var teacherUserId = teacherById.TryGetValue(offering.TeacherId, out var t) ? t.UserId : admin.Id;
                sessions.Add((session, teacherUserId, day, startUtc));
            }
        }

        // A demo file from before excuse letters existed has no table for them until first use.
        bool hasExcuseTable;
        try
        {
            _ = context.StudentExcuseRequests.Any();
            hasExcuseTable = true;
        }
        catch (Exception)
        {
            hasExcuseTable = false;
        }

        context.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            if (hasExcuseTable)
                context.StudentExcuseRequests.AddRange(excuses);
            context.LeaveRequests.AddRange(leaves);
            context.AttendanceSessions.AddRange(sessions.Select(s => s.Session));
            context.SaveChanges(); // assigns session Ids

            // ── Records: one per enrolled student per session ──
            var records = new List<AttendanceRecord>(sessions.Count * 10);
            foreach (var (session, teacherUserId, day, startUtc) in sessions)
            {
                var offering = offeringById[session.ClassOfferingId];
                if (!studentsBySection.TryGetValue(offering.SectionId, out var roster)) continue;

                var dayFactor = day == badDay ? 4.0
                    : day.DayOfWeek == DayOfWeek.Friday ? 1.6
                    : day.DayOfWeek == DayOfWeek.Monday ? 1.4
                    : 1.0;

                foreach (var studentId in roster)
                {
                    string status;
                    string? reason = null;
                    var excuse = excusedDays.TryGetValue(studentId, out var ranges)
                        ? ranges.FirstOrDefault(r => day >= r.From && day <= r.To)
                        : default;

                    if (excuse.Reason is not null)
                    {
                        status = "EXCUSED";
                        reason = excuse.Reason;
                    }
                    else if ((onStreak.Contains(studentId) && day >= streakFrom) || rng.NextDouble() < absenceChance[studentId] * dayFactor)
                    {
                        status = "ABSENT";
                        reason = AbsentReason;
                    }
                    else
                    {
                        status = rng.NextDouble() < 0.07 ? "LATE" : "PRESENT";
                    }

                    // CreatedAt doubles as the scan time: latecomers scan after the grace period.
                    var scanned = status switch
                    {
                        "PRESENT" => startUtc.AddSeconds(rng.Next(30, 9 * 60)),
                        "LATE" => startUtc.AddSeconds(rng.Next(16 * 60, 35 * 60)),
                        _ => session.UpdatedAt,
                    };
                    records.Add(new AttendanceRecord
                    {
                        AttendanceSessionId = session.Id,
                        StudentId = studentId,
                        MarkedByUserId = teacherUserId,
                        Status = status,
                        Reason = reason,
                        CreatedAt = scanned,
                        UpdatedAt = scanned,
                    });
                }
            }
            context.AttendanceRecords.AddRange(records);

            // ── Audit trail: sign-ins and some of the last week's sessions ──
            foreach (var day in days)
                Log(ToUtc(day, new TimeOnly(7, rng.Next(0, 25))), admin.Id, admin.Username, "SUPERADMIN", "Login", "User", admin.Id,
                    $"User '{admin.Username}' logged in successfully.");
            foreach (var (session, teacherUserId, _, startUtc) in sessions.Where(s => s.Day >= days[^5]))
            {
                if (rng.Next(5) != 0) continue;   // a sample, so sessions do not drown out everything else

                var offering = offeringById[session.ClassOfferingId];
                var name = teacherById.TryGetValue(offering.TeacherId, out var t) ? $"{t.LastName}, {t.FirstName}" : $"User#{teacherUserId}";
                Log(startUtc, offering.TeacherId, name, "TEACHER", "SessionStart", "AttendanceSession", session.Id,
                    $"Session started: {session.SectionName} - {session.SubjectName}");
                Log(session.UpdatedAt, teacherUserId, name, "TEACHER", "SessionEnd", "AttendanceSession", session.Id,
                    $"Session #{session.Id} ended ({session.SectionName} - {session.SubjectName}).");
            }
            context.AuditLogs.AddRange(audit.OrderBy(a => a.CreatedAt));

            context.SaveChanges();
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }

    /// <summary>Session and record times are stored as UTC; the demo is laid out in local school hours.</summary>
    private static DateTime ToUtc(DateOnly day, TimeOnly time) =>
        DateTime.SpecifyKind(day.ToDateTime(time), DateTimeKind.Local).ToUniversalTime();
}
