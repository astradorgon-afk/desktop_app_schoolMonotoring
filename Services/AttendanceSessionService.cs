using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class AttendanceSessionService : IAttendanceSessionService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public AttendanceSessionService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<TeacherOfferingItem>> GetTeacherOfferingsAsync(long teacherId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var today = DateTime.Now;
        var dbDay = (byte)(((int)today.DayOfWeek + 6) % 7 + 1);

        var activeSchoolYear = await context.SchoolYears
            .Where(sy => sy.Status == "ACTIVE")
            .Select(sy => sy.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var offerings = await context.ClassOfferings
            .Where(co => co.TeacherId == teacherId)
            .Where(co => co.SchoolYearId == activeSchoolYear)
            .Select(co => new
            {
                co.Id,
                co.SectionId,
                co.SubjectId,
                co.Room,
                SectionName = co.Section!.Name,
                GradeLevelId = co.Section!.GradeLevelId,
                SubjectTitle = co.Subject!.Title
            })
            .ToListAsync(cancellationToken);

        if (offerings.Count == 0)
            return [];

        var gradeLevelIds = offerings.Select(o => o.GradeLevelId).Distinct().ToList();
        var gradeLevels = await context.GradeLevels
            .Where(g => gradeLevelIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, g => g.Name, cancellationToken);

        var offeringIds = offerings.Select(o => o.Id).ToList();

        var todaySchedules = await context.ClassSchedules
            .Where(cs => offeringIds.Contains(cs.ClassOfferingId))
            .Where(cs => cs.DayOfWeek == dbDay)
            .Select(cs => new
            {
                cs.ClassOfferingId,
                cs.StartTime,
                cs.EndTime
            })
            .ToListAsync(cancellationToken);

        var scheduleByOffering = todaySchedules
            .GroupBy(s => s.ClassOfferingId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var todayStart = today.Date.ToUniversalTime();
        var todayEnd = todayStart.AddDays(1);

        var existingSessions = await context.AttendanceSessions
            .Where(s => s.SessionDate >= todayStart && s.SessionDate < todayEnd)
            .Where(s => offeringIds.Contains(s.ClassOfferingId))
            .Select(s => new { s.ClassOfferingId, s.Id })
            .ToListAsync(cancellationToken);

        var sessionByOffering = existingSessions
            .GroupBy(s => s.ClassOfferingId)
            .ToDictionary(g => g.Key, g => g.First().Id);

        var result = new List<TeacherOfferingItem>();

        foreach (var off in offerings)
        {
            if (!scheduleByOffering.TryGetValue(off.Id, out var schedules))
                continue;

            var times = schedules
                .OrderBy(s => s.StartTime)
                .Select(s => $"{s.StartTime:hh\\:mm}-{s.EndTime:hh\\:mm}")
                .ToList();

            var sessionId = sessionByOffering.GetValueOrDefault(off.Id);

            result.Add(new TeacherOfferingItem
            {
                OfferingId = off.Id,
                SectionId = off.SectionId,
                SectionName = off.SectionName,
                GradeLevel = gradeLevels.GetValueOrDefault(off.GradeLevelId, "—"),
                Subject = off.SubjectTitle,
                Room = off.Room,
                TimeDisplay = string.Join(", ", times),
                ExistingSessionId = sessionId != 0 ? sessionId : null
            });
        }

        return result.OrderBy(r => r.GradeLevel).ThenBy(r => r.SectionName).ThenBy(r => r.Subject).ToList();
    }

    public async Task<long> GetOrCreateSessionAsync(long classOfferingId, long userId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var todayStart = DateTime.Today.ToUniversalTime();
        var todayEnd = todayStart.AddDays(1);

        var existing = await context.AttendanceSessions
            .Where(s => s.ClassOfferingId == classOfferingId)
            .Where(s => s.SessionDate >= todayStart && s.SessionDate < todayEnd)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
            return existing.Id;

        var session = new AttendanceSession
        {
            ClassOfferingId = classOfferingId,
            SessionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.AttendanceSessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);

        return session.Id;
    }

    public async Task<List<SessionStudentItem>> GetSessionStudentsAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var session = await context.AttendanceSessions
            .Where(s => s.Id == sessionId)
            .Select(s => new { s.ClassOfferingId })
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
            return [];

        var offering = await context.ClassOfferings
            .Where(co => co.Id == session.ClassOfferingId)
            .Select(co => new { co.SectionId, co.SchoolYearId })
            .FirstOrDefaultAsync(cancellationToken);

        if (offering is null)
            return [];

        var records = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId)
            .Select(r => new { r.StudentId, r.Status })
            .ToListAsync(cancellationToken);

        var recordMap = records.ToDictionary(r => r.StudentId, r => r.Status);

        var students = await context.Enrollments
            .Where(e => e.SectionId == offering.SectionId)
            .Where(e => e.Status == "ACTIVE")
            .Where(e => e.SchoolYearId == offering.SchoolYearId)
            .Join(context.Students,
                e => e.StudentId,
                s => s.Id,
                (e, s) => new SessionStudentItem
                {
                    StudentId = s.Id,
                    FullName = s.LastName + ", " + s.FirstName + (s.MiddleName != null ? " " + s.MiddleName : ""),
                    Lrn = s.Lrn,
                    StudentNumber = s.StudentNumber,
                    Status = recordMap.GetValueOrDefault(s.Id)
                })
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);

        return students;
    }

    public async Task MarkAttendanceAsync(long sessionId, long studentId, long userId, string status, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId)
            .Where(r => r.StudentId == studentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            existing.Status = status;
            existing.MarkedByUserId = userId;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            context.AttendanceRecords.Add(new AttendanceRecord
            {
                AttendanceSessionId = sessionId,
                StudentId = studentId,
                MarkedByUserId = userId,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllPresentAsync(long sessionId, long userId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var students = await GetSessionStudentsAsync(sessionId, cancellationToken);
        var unmarked = students.Where(s => s.Status is null).ToList();

        foreach (var student in unmarked)
        {
            context.AttendanceRecords.Add(new AttendanceRecord
            {
                AttendanceSessionId = sessionId,
                StudentId = student.StudentId,
                MarkedByUserId = userId,
                Status = "PRESENT",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (unmarked.Count > 0)
            await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetMarkedCountAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.AttendanceRecords
            .CountAsync(r => r.AttendanceSessionId == sessionId, cancellationToken);
    }
}
