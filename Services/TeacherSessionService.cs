using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class TeacherSessionService : ITeacherSessionService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    private static readonly ConcurrentDictionary<string, CacheEntry<List<SectionSelectItem>>> _sectionCache = new();

    private sealed record CacheEntry<T>(T Value, DateTime CreatedAt)
    {
        public bool IsExpired => (DateTime.UtcNow - CreatedAt).TotalSeconds > 30;
    }

    public TeacherSessionService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<SectionSelectItem>> GetTeacherSectionsAsync(long teacherId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"sections_{teacherId}";
        if (_sectionCache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
            return cached.Value;

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var activeYearTask = context.SchoolYears
            .Where(sy => sy.Status == "ACTIVE")
            .Select(sy => sy.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var today = DateTime.Now;
        var dbDay = (byte)(((int)today.DayOfWeek + 6) % 7 + 1);

        var activeYear = await activeYearTask;

        var offerings = await context.ClassOfferings
            .Where(co => co.TeacherId == teacherId && co.SchoolYearId == activeYear)
            .Select(co => new
            {
                co.Id,
                co.SectionId,
                SectionName = co.Section!.Name,
                co.Section.GradeLevelId,
                SubjectTitle = co.Subject!.Title,
            })
            .ToListAsync(cancellationToken);

        if (offerings.Count == 0)
        {
            var empty = new List<SectionSelectItem>();
            _sectionCache[cacheKey] = new CacheEntry<List<SectionSelectItem>>(empty, DateTime.UtcNow);
            return empty;
        }

        var sectionIds = offerings.Select(o => o.SectionId).Distinct().ToList();
        var gradeLevelIds = offerings.Select(o => o.GradeLevelId).Distinct().ToList();
        var offeringIds = offerings.Select(o => o.Id).ToList();

        var gradeLevels = await context.GradeLevels
            .Where(g => gradeLevelIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, g => g.Name, cancellationToken);

        var studentCounts = await context.Enrollments
            .Where(e => e.Status == "ACTIVE" && e.SchoolYearId == activeYear && sectionIds.Contains(e.SectionId))
            .GroupBy(e => e.SectionId)
            .Select(g => new { SectionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.SectionId, g => g.Count, cancellationToken);

        var todaySchedules = await context.ClassSchedules
            .Where(cs => offeringIds.Contains(cs.ClassOfferingId) && cs.DayOfWeek == dbDay)
            .Select(cs => new { cs.ClassOfferingId, cs.StartTime, cs.EndTime })
            .ToListAsync(cancellationToken);

        var timesByOffering = todaySchedules
            .GroupBy(s => s.ClassOfferingId)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.StartTime).ToList());

        var result = new List<SectionSelectItem>(offerings.Count);

        foreach (var offering in offerings)
        {
            var hasSchedules = timesByOffering.TryGetValue(offering.Id, out var schedules);
            var timeDisplay = hasSchedules && schedules!.Count > 0
                ? string.Join(", ", schedules.Select(s => $"{s.StartTime:h\\:mm tt}-{s.EndTime:h\\:mm tt}"))
                : null;

            result.Add(new SectionSelectItem
            {
                OfferingId = offering.Id,
                SectionId = offering.SectionId,
                SectionName = offering.SectionName,
                GradeLevel = gradeLevels.GetValueOrDefault(offering.GradeLevelId, "—"),
                Subject = offering.SubjectTitle,
                TimeDisplay = timeDisplay,
                StudentCount = studentCounts.GetValueOrDefault(offering.SectionId, 0),
            });
        }

        result = result.OrderBy(r => r.GradeLevel).ThenBy(r => r.SectionName).ThenBy(r => r.Subject).ToList();
        _sectionCache[cacheKey] = new CacheEntry<List<SectionSelectItem>>(result, DateTime.UtcNow);
        return result;
    }

    public async Task<ActiveSessionState> StartManualSessionAsync(long teacherId, long userId, long offeringId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var offering = await context.ClassOfferings
            .Where(co => co.Id == offeringId && co.TeacherId == teacherId)
            .Select(co => new
            {
                co.Id,
                co.SectionId,
                co.Section!.Name,
                co.Subject!.Title,
                co.Room,
                co.SchoolYearId
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Offering not found or not assigned to you.");

        var todayStart = DateTime.Today.ToUniversalTime();
        var todayEnd = todayStart.AddDays(1);

        var stale = await context.AttendanceSessions
            .Where(s => s.ClassOfferingId == offeringId && s.SessionDate >= todayStart && s.SessionDate < todayEnd && s.Status == "ACTIVE")
            .ToListAsync(cancellationToken);

        foreach (var s in stale)
        {
            s.Status = "ENDED";
            s.UpdatedAt = DateTime.UtcNow;
        }

        var roomEntity = await context.Rooms
            .Where(r => r.Code == offering.Room || r.Name == offering.Room)
            .FirstOrDefaultAsync(cancellationToken);

        var session = new AttendanceSession
        {
            ClassOfferingId = offering.Id,
            SessionDate = DateTime.UtcNow,
            Status = "ACTIVE",
            RoomId = roomEntity?.Id,
            SectionName = offering.Name,
            SubjectName = offering.Title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.AttendanceSessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);

        return await BuildSessionStateAsync(session.Id, context, cancellationToken)
            ?? throw new InvalidOperationException("Failed to build session state.");
    }

    public async Task<ScanResult> ScanRoomQrAsync(long teacherId, long userId, string token, string? borrowPurpose = null, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var qrToken = await context.QrTokens
            .Where(q => q.Token == token && !q.IsRevoked && q.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (qrToken is null)
            return new ScanResult { Success = false, Message = "Invalid or expired QR code." };

        if (qrToken.ModelType != QrTokenModelType.ROOM)
            return new ScanResult { Success = false, Message = "This QR code is not a room code." };

        var roomId = qrToken.ModelId;
        var room = await context.Rooms.FindAsync([roomId], cancellationToken);
        if (room is null)
            return new ScanResult { Success = false, Message = "Room not found." };

        var today = DateTime.Now;
        var dbDay = (byte)(((int)today.DayOfWeek + 6) % 7 + 1);
        var todayStart = today.Date.ToUniversalTime();

        var schedule = await context.ClassSchedules
            .Where(cs => cs.RoomId == roomId && cs.DayOfWeek == dbDay)
            .Select(cs => new
            {
                cs.ClassOfferingId,
                cs.ClassOffering!.TeacherId,
                cs.ClassOffering!.Section!.Name,
                cs.ClassOffering!.Subject!.Title,
                cs.ClassOffering.Room,
                cs.ClassOffering.SchoolYearId
            })
            .FirstOrDefaultAsync(cancellationToken);

        long offeringId;
        string sectionName, subjectName;

        if (schedule is not null && schedule.TeacherId == teacherId)
        {
            offeringId = schedule.ClassOfferingId;
            sectionName = schedule.Name;
            subjectName = schedule.Title;
        }
        else if (!string.IsNullOrWhiteSpace(borrowPurpose))
        {
            var fallback = await context.ClassOfferings
                .Where(co => co.TeacherId == teacherId)
                .Select(co => new { co.Id, co.Section!.Name, co.Subject!.Title })
                .FirstOrDefaultAsync(cancellationToken);

            if (fallback is null)
                return new ScanResult { Success = false, Message = "No assignment found for this room/time." };

            offeringId = fallback.Id;
            sectionName = fallback.Name;
            subjectName = fallback.Title;
        }
        else
        {
            return new ScanResult { Success = false, Message = "This room is not assigned to you. Provide a borrow purpose to continue." };
        }

        var stale = await context.AttendanceSessions
            .Where(s => s.RoomId == roomId && s.Status == "ACTIVE")
            .ToListAsync(cancellationToken);

        foreach (var s in stale)
        {
            s.Status = "ENDED";
            s.UpdatedAt = DateTime.UtcNow;
        }

        var session = new AttendanceSession
        {
            ClassOfferingId = offeringId,
            SessionDate = DateTime.UtcNow,
            Status = "ACTIVE",
            RoomId = roomId,
            SectionName = sectionName,
            SubjectName = subjectName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.AttendanceSessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);

        return new ScanResult
        {
            Success = true,
            Message = schedule is not null && schedule.TeacherId == teacherId
                ? $"Session started for {sectionName}"
                : $"Borrowed session started for {sectionName}"
        };
    }

    public async Task<ActiveSessionState?> GetActiveSessionForTeacherAsync(long teacherId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var todayStart = DateTime.Today.ToUniversalTime();
        var todayEnd = todayStart.AddDays(1);

        var session = await context.AttendanceSessions
            .Where(s => s.ClassOffering!.TeacherId == teacherId
                && s.SessionDate >= todayStart
                && s.SessionDate < todayEnd
                && s.Status == "ACTIVE")
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (session == 0) return null;

        return await BuildSessionStateAsync(session, context, cancellationToken);
    }

    public async Task<ActiveSessionState?> GetActiveSessionAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await BuildSessionStateAsync(sessionId, context, cancellationToken);
    }

    public async Task<ScanResult> MarkAttendanceAsync(long sessionId, long userId, long studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var session = await context.AttendanceSessions.FindAsync([sessionId], cancellationToken);
        if (session is null || session.Status != "ACTIVE")
            return new ScanResult { Success = false, Message = "No active session." };

        var recent = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId && r.StudentId == studentId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (recent is not null && (DateTime.UtcNow - recent.CreatedAt).TotalMinutes < 2)
            return new ScanResult { Success = false, Message = $"Already marked as {recent.Status} less than 2 minutes ago." };

        var isLate = await IsStudentLateAsync(context, session.ClassOfferingId, cancellationToken);

        var status = isLate ? "LATE" : "PRESENT";
        var hash = ComputeHash(sessionId, studentId, DateTime.UtcNow);

        var record = new AttendanceRecord
        {
            AttendanceSessionId = sessionId,
            StudentId = studentId,
            MarkedByUserId = userId,
            Status = status,
            Hash = hash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.AttendanceRecords.Add(record);

        var student = await context.Students.FindAsync([studentId], cancellationToken);
        var fullName = student is not null ? $"{student.LastName}, {student.FirstName}" : "Unknown";

        await context.SaveChangesAsync(cancellationToken);

        return new ScanResult
        {
            Success = true,
            Message = $"{fullName} marked {status}",
            Status = status,
            StudentName = fullName,
            StudentId = studentId
        };
    }

    public async Task<ScanResult> ScanStudentQrAsync(long sessionId, long userId, string token, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var qrToken = await context.QrTokens
            .Where(q => q.Token == token && !q.IsRevoked && q.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (qrToken is null)
            return new ScanResult { Success = false, Message = "Invalid or expired QR code." };

        if (qrToken.ModelType != QrTokenModelType.STUDENT)
            return new ScanResult { Success = false, Message = "This is not a student QR code." };

        return await MarkAttendanceAsync(sessionId, userId, qrToken.ModelId, cancellationToken);
    }

    public async Task<ScanResult> UndoAttendanceAsync(long sessionId, long userId, long studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var session = await context.AttendanceSessions.FindAsync([sessionId], cancellationToken);
        if (session is null || session.Status != "ACTIVE")
            return new ScanResult { Success = false, Message = "No active session." };

        var mostRecent = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId && r.StudentId == studentId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (mostRecent is null)
            return new ScanResult { Success = false, Message = "No attendance record to undo." };

        var student = await context.Students.FindAsync([studentId], cancellationToken);
        var fullName = student is not null ? $"{student.LastName}, {student.FirstName}" : "Unknown";

        context.AttendanceRecords.Remove(mostRecent);
        await context.SaveChangesAsync(cancellationToken);

        return new ScanResult
        {
            Success = true,
            Message = $"Removed {mostRecent.Status} mark for {fullName}",
            StudentName = fullName
        };
    }

    public async Task EndSessionAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var session = await context.AttendanceSessions.FindAsync([sessionId], cancellationToken);
        if (session is null) return;

        session.Status = "ENDED";
        session.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<LiveScanEvent>> GetLiveScansAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var records = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(50)
            .Select(r => new
            {
                r.StudentId,
                r.Status,
                r.CreatedAt,
                StudentName = r.Student!.LastName + ", " + r.Student.FirstName
            })
            .ToListAsync(cancellationToken);

        var stats = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId)
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var present = stats.FirstOrDefault(s => s.Status == "PRESENT")?.Count ?? 0;
        var late = stats.FirstOrDefault(s => s.Status == "LATE")?.Count ?? 0;
        var absent = stats.FirstOrDefault(s => s.Status == "ABSENT")?.Count ?? 0;

        return records.Select(r => new LiveScanEvent
        {
            StudentId = r.StudentId,
            FullName = r.StudentName,
            Status = r.Status,
            Timestamp = r.CreatedAt,
            PresentCount = present,
            LateCount = late,
            AbsentCount = absent
        }).ToList();
    }

    public async Task<List<RosterStudentItem>> GetRosterAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var session = await context.AttendanceSessions.FindAsync([sessionId], cancellationToken);
        if (session is null) return [];

        var offering = await context.ClassOfferings.FindAsync([session.ClassOfferingId], cancellationToken);
        if (offering is null) return [];

        var activeYear = await context.SchoolYears
            .Where(sy => sy.Status == "ACTIVE")
            .Select(sy => sy.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var records = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId)
            .ToDictionaryAsync(r => r.StudentId, r => r, cancellationToken);

        var raw = await context.Enrollments
            .Where(e => e.Status == "ACTIVE" && e.SchoolYearId == activeYear && e.SectionId == offering.SectionId)
            .Join(context.Students,
                e => e.StudentId,
                s => s.Id,
                (e, s) => new { s.Id, FullName = s.LastName + ", " + s.FirstName, Lrn = s.Lrn ?? "", s.StudentNumber })
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return raw.Select(s => new RosterStudentItem
        {
            StudentId = s.Id,
            FullName = s.FullName,
            Lrn = s.Lrn,
            StudentNumber = s.StudentNumber ?? "",
            Status = records.TryGetValue(s.Id, out var rec) ? rec.Status : null,
            ScannedAt = records.TryGetValue(s.Id, out var rec2) ? rec2.CreatedAt : null,
        }).ToList();
    }

    public async Task<List<SessionHistoryItem>> GetAttendanceHistoryAsync(long teacherId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var fromDate = from?.ToDateTime(TimeOnly.MinValue).ToUniversalTime() ?? DateTime.Today.AddDays(-30).ToUniversalTime();
        var toDate = to?.ToDateTime(TimeOnly.MaxValue).ToUniversalTime() ?? DateTime.Today.AddDays(1).ToUniversalTime();

        var sessions = await context.AttendanceSessions
            .Where(s => s.ClassOffering!.TeacherId == teacherId
                && s.SessionDate >= fromDate
                && s.SessionDate <= toDate)
            .OrderByDescending(s => s.SessionDate)
            .ThenByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.SectionName,
                s.SubjectName,
                s.RoomId,
                s.SessionDate,
                s.Status,
                s.ClassOfferingId,
                Enrolled = context.Enrollments
                    .Count(e => e.Status == "ACTIVE" && e.SectionId == s.ClassOffering!.SectionId
                        && e.SchoolYearId == s.ClassOffering.SchoolYearId),
                Present = context.AttendanceRecords.Count(r => r.AttendanceSessionId == s.Id && r.Status == "PRESENT"),
                Late = context.AttendanceRecords.Count(r => r.AttendanceSessionId == s.Id && r.Status == "LATE"),
                Absent = context.AttendanceRecords.Count(r => r.AttendanceSessionId == s.Id && r.Status == "ABSENT"),
            })
            .ToListAsync(cancellationToken);

        return sessions.Select(s => new SessionHistoryItem
        {
            SessionId = s.Id,
            SectionName = s.SectionName ?? "",
            SubjectName = s.SubjectName ?? "",
            Room = s.RoomId?.ToString(),
            SessionDate = s.SessionDate,
            Status = s.Status,
            Enrolled = s.Enrolled,
            Present = s.Present,
            Late = s.Late,
            Absent = s.Absent,
            Unmarked = s.Enrolled - s.Present - s.Late - s.Absent,
        }).ToList();
    }

    public async Task<List<SessionHistoryStudent>> GetSessionStudentsAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var session = await context.AttendanceSessions.FindAsync([sessionId], cancellationToken);
        if (session is null) return [];

        var offering = await context.ClassOfferings.FindAsync([session.ClassOfferingId], cancellationToken);
        if (offering is null) return [];

        var activeYear = await context.SchoolYears
            .Where(sy => sy.Status == "ACTIVE")
            .Select(sy => sy.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var records = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId)
            .ToDictionaryAsync(r => r.StudentId, r => r, cancellationToken);

        var raw = await context.Enrollments
            .Where(e => e.Status == "ACTIVE" && e.SchoolYearId == activeYear && e.SectionId == offering.SectionId)
            .Join(context.Students,
                e => e.StudentId,
                s => s.Id,
                (e, s) => new { s.Id, FullName = s.LastName + ", " + s.FirstName, s.Lrn })
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return raw.Select(s => new SessionHistoryStudent
        {
            StudentId = s.Id,
            FullName = s.FullName,
            Lrn = s.Lrn ?? "",
            Status = records.TryGetValue(s.Id, out var rec) ? rec.Status : null,
            ScannedAt = records.TryGetValue(s.Id, out var rec2) ? rec2.CreatedAt : null,
        }).ToList();
    }

    // ── Private helpers ──

    private static async Task<ActiveSessionState?> BuildSessionStateAsync(long sessionId, SchoolDbContext context, CancellationToken ct)
    {
        var session = await context.AttendanceSessions.FindAsync([sessionId], ct);
        if (session is null) return null;

        var records = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId)
            .ToListAsync(ct);

        var total = await context.Enrollments
            .Where(e => e.SectionId == context.ClassOfferings
                .Where(co => co.Id == session.ClassOfferingId)
                .Select(co => co.SectionId)
                .First())
            .Where(e => e.Status == "ACTIVE")
            .CountAsync(ct);

        var present = records.Count(r => r.Status == "PRESENT");
        var late = records.Count(r => r.Status == "LATE");
        var absent = records.Count(r => r.Status == "ABSENT");

        return new ActiveSessionState
        {
            SessionId = session.Id,
            OfferingId = session.ClassOfferingId,
            SectionName = session.SectionName ?? "",
            SubjectName = session.SubjectName ?? "",
            Room = session.RoomId?.ToString(),
            StartedAt = session.CreatedAt,
            TotalStudents = total,
            PresentCount = present,
            LateCount = late,
            AbsentCount = absent,
            UnmarkedCount = total - present - late - absent
        };
    }

    private static async Task<bool> IsStudentLateAsync(SchoolDbContext context, long offeringId, CancellationToken ct)
    {
        var today = DateTime.Now;
        var dbDay = (byte)(((int)today.DayOfWeek + 6) % 7 + 1);

        var schedule = await context.ClassSchedules
            .Where(cs => cs.ClassOfferingId == offeringId && cs.DayOfWeek == dbDay)
            .OrderBy(cs => cs.StartTime)
            .FirstOrDefaultAsync(ct);

        if (schedule is null) return false;

        var graceEnd = schedule.StartTime.Add(new TimeSpan(0, 15, 0));
        var now = TimeOnly.FromDateTime(today);

        return now > graceEnd;
    }

    private static string ComputeHash(long sessionId, long studentId, DateTime timestamp)
    {
        var input = $"{sessionId}:{studentId}:{timestamp:O}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
