using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class LiveMonitoringService : ILiveMonitoringService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public LiveMonitoringService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<RoomStatusItem>> GetTodayStatusAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var today = DateTime.Today;
        var dayOfWeek = ToDbDayOfWeek(today.DayOfWeek);
        var todayStart = today.ToUniversalTime();
        var todayEnd = todayStart.AddDays(1);

        // Get all active rooms
        var rooms = await context.Rooms
            .Where(r => r.IsActive)
            .OrderBy(r => r.Code)
            .ToListAsync(ct);

        // Find schedules for today
        var todaySchedules = await context.ClassSchedules
            .Where(cs => cs.DayOfWeek == dayOfWeek)
            .Select(cs => new
            {
                cs.Id,
                cs.ClassOfferingId,
                cs.StartTime,
                cs.EndTime,
                OfferingRoom = cs.ClassOffering!.Room ?? "",
                OfferingSectionId = cs.ClassOffering!.SectionId,
                SectionName = cs.ClassOffering!.Section!.Name,
                TeacherId = cs.ClassOffering!.Teacher!.Id,
                TeacherName = cs.ClassOffering!.Teacher!.FirstName + " " + cs.ClassOffering!.Teacher!.LastName,
                SubjectTitle = cs.ClassOffering!.Subject!.Title
            })
            .ToListAsync(ct);

        // Load teacher profile pictures once (kept out of the projection above so the
        // image blobs aren't duplicated across every schedule row).
        var teacherIds = todaySchedules.Select(s => s.TeacherId).Distinct().ToList();
        var teacherPhotos = teacherIds.Count > 0
            ? await context.Teachers
                .Where(t => teacherIds.Contains(t.Id) && t.ProfilePicture != null)
                .Select(t => new { t.Id, t.ProfilePicture })
                .ToDictionaryAsync(t => t.Id, t => t.ProfilePicture, ct)
            : [];

        // Group by room (offerings can have multiple schedules, one per day)
        var schedulesByRoom = todaySchedules
            .GroupBy(s => s.OfferingRoom)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Get enrollment counts per section
        var sectionIds = todaySchedules.Select(s => s.OfferingSectionId).Distinct().ToList();
        var enrollmentCounts = sectionIds.Count > 0
            ? await context.Enrollments
                .Where(e => sectionIds.Contains(e.SectionId) && e.Status == "ENROLLED")
                .GroupBy(e => e.SectionId)
                .Select(g => new { SectionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.SectionId, g => g.Count, ct)
            : [];

        // Get today's attendance sessions and their record counts
        var attendanceSessions = await context.AttendanceSessions
            .Where(s => s.SessionDate >= todayStart && s.SessionDate < todayEnd)
            .Where(s => s.Status == "ACTIVE")
            .Select(s => new
            {
                s.Id,
                s.ClassOfferingId,
                PresentCount = s.Records.Count(r => r.Status == "PRESENT" || r.Status == "LATE"),
                LateCount = s.Records.Count(r => r.Status == "LATE"),
                AbsentCount = s.Records.Count(r => r.Status == "ABSENT")
            })
            .ToListAsync(ct);

        var sessionByOffering = attendanceSessions
            .GroupBy(s => s.ClassOfferingId)
            .ToDictionary(g => g.Key, g => g.First());

        var items = new List<RoomStatusItem>();

        foreach (var room in rooms)
        {
            var matched = schedulesByRoom.TryGetValue(room.Code, out var schedules);
            if (!matched || schedules is null || schedules.Count == 0)
            {
                items.Add(new RoomStatusItem
                {
                    RoomId = room.Id,
                    RoomCode = room.Code,
                    RoomName = room.Name,
                    Capacity = room.Capacity,
                    HasSession = false
                });
                continue;
            }

            // Pick the schedule most relevant to right now, so a room with several
            // classes today surfaces the one that matches the current time:
            //   1. a class currently in session (start <= now < end)
            //   2. otherwise the next class still to come today
            //   3. otherwise the most recent class that already finished
            var now = TimeOnly.FromDateTime(DateTime.Now);
            var schedule =
                schedules.FirstOrDefault(s => now >= s.StartTime && now < s.EndTime)
                ?? schedules.Where(s => s.StartTime > now).OrderBy(s => s.StartTime).FirstOrDefault()
                ?? schedules.OrderByDescending(s => s.EndTime).First();
            var totalStudents = enrollmentCounts.GetValueOrDefault(schedule.OfferingSectionId, 0);
            var hasSession = sessionByOffering.TryGetValue(schedule.ClassOfferingId, out var session);

            items.Add(new RoomStatusItem
            {
                RoomId = room.Id,
                RoomCode = room.Code,
                RoomName = room.Name,
                Capacity = room.Capacity,
                OfferingId = schedule.ClassOfferingId,
                SectionName = schedule.SectionName,
                TeacherId = schedule.TeacherId,
                TeacherName = schedule.TeacherName,
                TeacherPhoto = teacherPhotos.GetValueOrDefault(schedule.TeacherId),
                SubjectTitle = schedule.SubjectTitle,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                TotalStudents = totalStudents,
                PresentCount = hasSession ? session!.PresentCount : 0,
                LateCount = hasSession ? session!.LateCount : 0,
                AbsentCount = hasSession ? session!.AbsentCount : 0,
                HasSession = hasSession
            });
        }

        return items;
    }

    private static byte ToDbDayOfWeek(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => 1,
        DayOfWeek.Tuesday => 2,
        DayOfWeek.Wednesday => 3,
        DayOfWeek.Thursday => 4,
        DayOfWeek.Friday => 5,
        DayOfWeek.Saturday => 6,
        DayOfWeek.Sunday => 7,
        _ => 0
    };
}
