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
    private readonly IAuditService _auditService;

    private static readonly ConcurrentDictionary<string, CacheEntry<List<SectionSelectItem>>> _sectionCache = new();

    private sealed record CacheEntry<T>(T Value, DateTime CreatedAt)
    {
        public bool IsExpired => (DateTime.UtcNow - CreatedAt).TotalSeconds > 30;
    }

    public TeacherSessionService(IDbContextFactory<SchoolDbContext> contextFactory, IAuditService auditService)
    {
        _contextFactory = contextFactory;
        _auditService = auditService;
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
                IsToday = hasSchedules && schedules!.Count > 0,
                StartsAt = hasSchedules && schedules!.Count > 0 ? schedules[0].StartTime : null,
            });
        }

        // Classes scheduled today come first, in the order they run; everything else the
        // teacher handles follows, so the picker still offers make-up and ad-hoc sessions.
        result = result
            .OrderByDescending(r => r.IsToday)
            .ThenBy(r => r.StartsAt ?? TimeOnly.MaxValue)
            .ThenBy(r => r.GradeLevel)
            .ThenBy(r => r.SectionName)
            .ThenBy(r => r.Subject)
            .ToList();
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

        // A teacher can only run one session at a time, so starting a new one closes every
        // session still open for this offering or for this teacher — on any date, not just
        // today. Previously this was scoped to today, which let sessions left running
        // overnight stay ACTIVE indefinitely.
        var stale = await context.AttendanceSessions
            .Where(s => s.Status == "ACTIVE"
                && (s.ClassOfferingId == offeringId || s.ClassOffering!.TeacherId == teacherId))
            .ToListAsync(cancellationToken);

        foreach (var s in stale)
        {
            s.Status = "ENDED";
            s.UpdatedAt = DateTime.UtcNow;
        }
        await MarkUnscannedAbsentAsync(context, stale, cancellationToken);

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

        var teacher = await context.Teachers.FindAsync([teacherId], cancellationToken);
        var teacherName = teacher is not null ? $"{teacher.LastName}, {teacher.FirstName}" : $"Teacher#{teacherId}";
        await _auditService.LogAsync(userId, teacherName, "TEACHER", "SessionStart", "AttendanceSession", session.Id,
            $"Session started: {offering.Name} - {offering.Title}");

        return await BuildSessionStateAsync(session.Id, context, cancellationToken)
            ?? throw new InvalidOperationException("Failed to build session state.");
    }

    public async Task<ScanResult> ScanRoomQrAsync(long teacherId, long userId, string token, string? borrowPurpose = null, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        long roomId;
        {
            var qrToken = await context.QrTokens
                .Where(q => q.Token == token && !q.IsRevoked && q.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);

            if (qrToken is not null)
            {
                if (qrToken.ModelType != QrTokenModelType.ROOM)
                    return new ScanResult { Success = false, Message = "This QR code is not a room code." };
                roomId = qrToken.ModelId;
            }
            else
            {
                var roomQr = await context.RoomQrCodes
                    .Where(q => q.QrContent == token)
                    .FirstOrDefaultAsync(cancellationToken);
                if (roomQr is not null)
                {
                    roomId = roomQr.RoomId;
                }
                else
                {
                    var studentQr = await context.StudentQrCodes
                        .Where(q => q.QrContent == token)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (studentQr is not null)
                        return new ScanResult { Success = false, Message = "Start a session first before scanning student QR codes." };
                    return new ScanResult { Success = false, Message = "Invalid or expired QR code." };
                }
            }
        }
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
        await MarkUnscannedAbsentAsync(context, stale, cancellationToken);

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

    /// <summary>
    /// Ends any session still flagged ACTIVE from a previous day. A session that is never
    /// closed — the app is shut down, or the teacher walks away without pressing End —
    /// otherwise keeps its ACTIVE status forever: the Start Session page ignores it (it only
    /// looks at today), but Attendance History reads the raw status and keeps showing it as
    /// live. Sweeping them here keeps the two views agreeing.
    /// </summary>
    public async Task<int> CloseStaleSessionsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await CloseStaleSessionsAsync(context, cancellationToken);
    }

    private static async Task<int> CloseStaleSessionsAsync(SchoolDbContext context, CancellationToken cancellationToken)
    {
        var todayStart = DateTime.Today.ToUniversalTime();

        var stale = await context.AttendanceSessions
            .Where(s => s.Status == "ACTIVE" && s.SessionDate < todayStart)
            .ToListAsync(cancellationToken);

        if (stale.Count == 0) return 0;

        foreach (var s in stale)
        {
            s.Status = "ENDED";
            s.UpdatedAt = DateTime.UtcNow;
        }
        await MarkUnscannedAbsentAsync(context, stale, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return stale.Count;
    }

    public async Task<ActiveSessionState?> GetActiveSessionForTeacherAsync(long teacherId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        await CloseStaleSessionsAsync(context, cancellationToken);

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

        var offering = await context.ClassOfferings.FindAsync([session.ClassOfferingId], cancellationToken);
        if (offering is null)
            return new ScanResult { Success = false, Message = "Class offering not found." };

        var activeYear = await context.SchoolYears
            .Where(sy => sy.Status == "ACTIVE")
            .Select(sy => sy.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var isEnrolled = await context.Enrollments
            .AnyAsync(e => e.StudentId == studentId
                && e.SectionId == offering.SectionId
                && e.SchoolYearId == activeYear
                && e.Status == "ACTIVE", cancellationToken);

        if (!isEnrolled)
            return new ScanResult { Success = false, Message = "Student is not enrolled in this class section." };

        var recent = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId && r.StudentId == studentId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // The database allows one record per student per session (unique key on MySQL), so a
        // repeat scan never inserts. Someone already present/late is reported as such; a
        // student a teacher had marked absent/excused who then arrives is upgraded in place.
        if (recent is not null && recent.Status is "PRESENT" or "LATE")
            return new ScanResult { Success = false, Message = $"Already marked as {recent.Status}." };

        var rules = await AttendanceSettingsService.ReadRulesAsync(context, cancellationToken);
        var isLate = await IsStudentLateAsync(context, session.ClassOfferingId, rules.LateGraceMinutes, cancellationToken);

        var status = isLate ? "LATE" : "PRESENT";
        var hash = ComputeHash(sessionId, studentId, DateTime.UtcNow);

        var record = recent;
        if (record is null)
        {
            record = new AttendanceRecord
            {
                AttendanceSessionId = sessionId,
                StudentId = studentId
            };
            context.AttendanceRecords.Add(record);
        }

        // CreatedAt doubles as the scan time shown on the roster and live feed.
        record.CreatedAt = DateTime.UtcNow;
        record.MarkedByUserId = userId;
        record.Status = status;
        record.Reason = null;
        record.Hash = hash;
        record.UpdatedAt = DateTime.UtcNow;

        var student = await context.Students.FindAsync([studentId], cancellationToken);
        var fullName = student is not null ? $"{student.LastName}, {student.FirstName}" : "Unknown";

        await context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(userId, $"User#{userId}", "TEACHER", "AttendanceMark", "AttendanceRecord", record.Id,
            $"{fullName} marked {status} in session #{sessionId}");

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

        if (qrToken is not null)
        {
            if (qrToken.ModelType != QrTokenModelType.STUDENT)
                return new ScanResult { Success = false, Message = "This is not a student QR code." };

            return await MarkAttendanceAsync(sessionId, userId, qrToken.ModelId, cancellationToken);
        }

        var studentQr = await context.StudentQrCodes
            .Where(q => q.QrContent == token)
            .FirstOrDefaultAsync(cancellationToken);

        if (studentQr is not null)
            return await MarkAttendanceAsync(sessionId, userId, studentQr.StudentId, cancellationToken);

        return new ScanResult { Success = false, Message = "Invalid or expired QR code." };
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

        await _auditService.LogAsync(userId, $"User#{userId}", "TEACHER", "AttendanceUndo", "AttendanceRecord", mostRecent.Id,
            $"Removed {mostRecent.Status} mark for {fullName} in session #{sessionId}");

        return new ScanResult
        {
            Success = true,
            Message = $"Removed {mostRecent.Status} mark for {fullName}",
            StudentName = fullName
        };
    }

    private static readonly string[] OverridableStatuses = ["PRESENT", "LATE", "ABSENT", "EXCUSED"];

    public async Task<ScanResult> OverrideStatusAsync(long sessionId, long teacherId, long userId, long studentId, string status, string reason, CancellationToken cancellationToken = default)
    {
        if (!OverridableStatuses.Contains(status))
            return new ScanResult { Success = false, Message = "Choose Present, Late, Absent or Excused." };
        if (string.IsNullOrWhiteSpace(reason))
            return new ScanResult { Success = false, Message = "Please give a reason for the correction." };

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var session = await context.AttendanceSessions.FindAsync([sessionId], cancellationToken);
        if (session is null)
            return new ScanResult { Success = false, Message = "Session not found." };

        var offering = await context.ClassOfferings.FindAsync([session.ClassOfferingId], cancellationToken);
        if (offering is null || offering.TeacherId != teacherId)
            return new ScanResult { Success = false, Message = "You can only correct attendance for your own classes." };

        var isEnrolled = await context.Enrollments.AnyAsync(e => e.StudentId == studentId
            && e.SectionId == offering.SectionId
            && e.SchoolYearId == offering.SchoolYearId
            && e.Status == "ACTIVE", cancellationToken);
        if (!isEnrolled)
            return new ScanResult { Success = false, Message = "Student is not enrolled in this class section." };

        var record = await context.AttendanceRecords
            .Where(r => r.AttendanceSessionId == sessionId && r.StudentId == studentId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var previous = record?.Status ?? "UNMARKED";
        var note = $"Corrected by teacher: {reason.Trim()}";
        if (note.Length > 250) note = note[..250];

        if (record is null)
        {
            record = new AttendanceRecord
            {
                AttendanceSessionId = sessionId,
                StudentId = studentId,
                Hash = ComputeHash(sessionId, studentId, now),
                CreatedAt = now
            };
            context.AttendanceRecords.Add(record);
        }

        record.Status = status;
        record.Reason = note;
        record.MarkedByUserId = userId;
        record.UpdatedAt = now;

        var student = await context.Students.FindAsync([studentId], cancellationToken);
        var fullName = student is not null ? $"{student.LastName}, {student.FirstName}" : "Unknown";

        await context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(userId, $"User#{userId}", "TEACHER", "AttendanceOverride", "AttendanceRecord", record.Id,
            $"{fullName}: {previous} → {status} in session #{sessionId}. Reason: {reason.Trim()}");

        return new ScanResult
        {
            Success = true,
            Message = $"{fullName} changed from {previous} to {status}.",
            Status = status,
            StudentName = fullName,
            StudentId = studentId
        };
    }

    public async Task EndSessionAsync(long sessionId, long? userId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var session = await context.AttendanceSessions.FindAsync([sessionId], cancellationToken);
        if (session is null) return;

        session.Status = "ENDED";
        session.UpdatedAt = DateTime.UtcNow;
        await MarkUnscannedAbsentAsync(context, [session], cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(userId ?? 0, userId is not null ? $"User#{userId}" : "System", "TEACHER", "SessionEnd", "AttendanceSession", sessionId,
            $"Session #{sessionId} ended ({session.SectionName} - {session.SubjectName}).");
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

        // Older SQLite data can hold two rows for a student (MySQL has a unique key); use the latest.
        var records = (await context.AttendanceRecords
                .Where(r => r.AttendanceSessionId == sessionId)
                .ToListAsync(cancellationToken))
            .GroupBy(r => r.StudentId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.CreatedAt).First());

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

        // Retire yesterday's abandoned sessions first, so history never shows a green
        // ACTIVE badge for a session the Start Session page no longer knows about.
        await CloseStaleSessionsAsync(context, cancellationToken);

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
                Excused = context.AttendanceRecords.Count(r => r.AttendanceSessionId == s.Id && r.Status == "EXCUSED"),
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
            Unmarked = s.Enrolled - s.Present - s.Late - s.Absent - s.Excused,
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

        // Older SQLite data can hold two rows for a student (MySQL has a unique key); use the latest.
        var records = (await context.AttendanceRecords
                .Where(r => r.AttendanceSessionId == sessionId)
                .ToListAsync(cancellationToken))
            .GroupBy(r => r.StudentId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.CreatedAt).First());

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

    public async Task<List<TeacherAttendanceDay>> GetTeacherAttendanceAsync(long teacherId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var toDate = to ?? DateOnly.FromDateTime(DateTime.Today);
        var fromDate = from ?? toDate.AddDays(-13);
        if (fromDate > toDate)
            (fromDate, toDate) = (toDate, fromDate);

        var activeYear = await context.SchoolYears
            .Where(sy => sy.Status == "ACTIVE")
            .Select(sy => sy.Id)
            .FirstOrDefaultAsync(cancellationToken);

        // The teacher's class offerings for the active year.
        var offerings = await context.ClassOfferings
            .Where(co => co.TeacherId == teacherId && co.SchoolYearId == activeYear)
            .Select(co => new
            {
                co.Id,
                SectionName = co.Section!.Name,
                SubjectTitle = co.Subject!.Title,
                co.Room
            })
            .ToListAsync(cancellationToken);

        if (offerings.Count == 0)
            return [];

        var offeringInfo = offerings.ToDictionary(o => o.Id, o => o);
        var offeringIds = offerings.Select(o => o.Id).ToList();

        // Weekly schedule rows (one per day-of-week) for those offerings.
        var schedules = await context.ClassSchedules
            .Where(cs => offeringIds.Contains(cs.ClassOfferingId))
            .Select(cs => new { cs.ClassOfferingId, cs.DayOfWeek, cs.StartTime, cs.EndTime })
            .ToListAsync(cancellationToken);

        if (schedules.Count == 0)
            return [];

        var schedulesByDay = schedules
            .GroupBy(s => s.DayOfWeek)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Sessions actually held in the range → set of (offering, local date) with the time held.
        var fromUtc = fromDate.ToDateTime(TimeOnly.MinValue).ToUniversalTime();
        var toUtc = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue).ToUniversalTime();

        var heldSessions = await context.AttendanceSessions
            .Where(s => s.ClassOffering!.TeacherId == teacherId
                && s.SessionDate >= fromUtc
                && s.SessionDate < toUtc)
            .Select(s => new { s.ClassOfferingId, s.SessionDate, s.CreatedAt })
            .ToListAsync(cancellationToken);

        var heldByKey = new Dictionary<(long, DateOnly), DateTime>();
        foreach (var s in heldSessions)
        {
            var localDate = DateOnly.FromDateTime(s.SessionDate.ToLocalTime());
            var key = (s.ClassOfferingId, localDate);
            var heldAt = s.CreatedAt.ToLocalTime();
            if (!heldByKey.TryGetValue(key, out var existing) || heldAt < existing)
                heldByKey[key] = heldAt;
        }

        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);

        var days = new List<TeacherAttendanceDay>();

        for (var date = toDate; date >= fromDate; date = date.AddDays(-1))
        {
            var dbDay = (byte)(((int)date.DayOfWeek + 6) % 7 + 1);
            if (!schedulesByDay.TryGetValue(dbDay, out var daySchedules))
                continue;

            var classes = new List<TeacherAttendanceClass>();
            foreach (var sched in daySchedules.OrderBy(s => s.StartTime))
            {
                if (!offeringInfo.TryGetValue(sched.ClassOfferingId, out var info))
                    continue;

                var held = heldByKey.TryGetValue((sched.ClassOfferingId, date), out var heldAt);

                string status;
                if (held)
                    status = "PRESENT";
                else if (date < today || (date == today && nowTime > sched.EndTime))
                    status = "ABSENT";
                else
                    status = "UPCOMING";

                classes.Add(new TeacherAttendanceClass
                {
                    OfferingId = sched.ClassOfferingId,
                    SectionName = info.SectionName,
                    SubjectName = info.SubjectTitle,
                    Room = info.Room,
                    StartTime = sched.StartTime,
                    EndTime = sched.EndTime,
                    Status = status,
                    HeldAt = held ? heldAt : null
                });
            }

            if (classes.Count > 0)
                days.Add(new TeacherAttendanceDay { Date = date, Classes = classes });
        }

        return days;
    }

    // ── Private helpers ──

    /// <summary>
    /// Records every enrolled student who was never scanned as ABSENT for sessions that are
    /// being ended. Without this an unscanned student simply had no record, so reports and
    /// attendance rates never counted absences. Students with an approved excuse covering the
    /// session date are recorded EXCUSED instead. Adds to the context; the caller saves.
    /// </summary>
    private static async Task MarkUnscannedAbsentAsync(SchoolDbContext context, IReadOnlyCollection<AttendanceSession> sessions, CancellationToken ct)
    {
        if (sessions.Count == 0) return;

        var offeringIds = sessions.Select(s => s.ClassOfferingId).Distinct().ToList();
        var offerings = await context.ClassOfferings
            .Where(o => offeringIds.Contains(o.Id))
            .Select(o => new { o.Id, o.SectionId, o.SchoolYearId })
            .ToDictionaryAsync(o => o.Id, ct);

        var sessionIds = sessions.Select(s => s.Id).ToList();
        var alreadyMarked = (await context.AttendanceRecords
                .Where(r => sessionIds.Contains(r.AttendanceSessionId))
                .Select(r => new { r.AttendanceSessionId, r.StudentId })
                .ToListAsync(ct))
            .Select(r => (r.AttendanceSessionId, r.StudentId))
            .ToHashSet();

        var now = DateTime.UtcNow;
        foreach (var session in sessions)
        {
            if (!offerings.TryGetValue(session.ClassOfferingId, out var offering)) continue;

            var enrolled = await context.Enrollments
                .Where(e => e.SectionId == offering.SectionId
                    && e.SchoolYearId == offering.SchoolYearId
                    && e.Status == "ACTIVE")
                .Select(e => e.StudentId)
                .Distinct()
                .ToListAsync(ct);

            var missing = enrolled.Where(id => !alreadyMarked.Contains((session.Id, id))).ToList();
            if (missing.Count == 0) continue;

            var sessionDay = DateOnly.FromDateTime(session.SessionDate.ToLocalTime());
            var excused = await StudentExcuseService.GetExcusedStudentsAsync(context, missing, sessionDay, ct);

            foreach (var studentId in missing)
            {
                var isExcused = excused.TryGetValue(studentId, out var excuseReason);
                context.AttendanceRecords.Add(new AttendanceRecord
                {
                    AttendanceSessionId = session.Id,
                    StudentId = studentId,
                    Status = isExcused ? "EXCUSED" : "ABSENT",
                    Reason = isExcused ? excuseReason : "Not scanned before the session ended",
                    Hash = ComputeHash(session.Id, studentId, now),
                    CreatedAt = now,
                    UpdatedAt = now
                });
                alreadyMarked.Add((session.Id, studentId));
            }
        }
    }

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

    private static async Task<bool> IsStudentLateAsync(SchoolDbContext context, long offeringId, int graceMinutes, CancellationToken ct)
    {
        var today = DateTime.Now;
        var dbDay = (byte)(((int)today.DayOfWeek + 6) % 7 + 1);

        var schedule = await context.ClassSchedules
            .Where(cs => cs.ClassOfferingId == offeringId && cs.DayOfWeek == dbDay)
            .OrderBy(cs => cs.StartTime)
            .FirstOrDefaultAsync(ct);

        if (schedule is null) return false;

        var graceEnd = schedule.StartTime.Add(TimeSpan.FromMinutes(graceMinutes));
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
