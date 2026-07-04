using System.Globalization;
using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class StudentPortalService : IStudentPortalService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private static readonly string[] DayNames = ["", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    public StudentPortalService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task<Student?> GetStudentByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Students
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<StudentDashboardData?> GetDashboardAsync(long studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        if (student is null) return null;

        // Find active enrollment
        var activeSchoolYear = await context.SchoolYears
            .Where(sy => sy.Status == "ACTIVE")
            .Select(sy => new { sy.Id, sy.Name })
            .FirstOrDefaultAsync(cancellationToken);

        string sectionName = "", gradeLevel = "", schoolYearName = "";

        Enrollment? enrollment = null;
        if (activeSchoolYear is not null)
        {
            schoolYearName = activeSchoolYear.Name;
            enrollment = await context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Where(e => e.SchoolYearId == activeSchoolYear.Id)
                .Where(e => e.Status == "ACTIVE")
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (enrollment is not null)
        {
            var section = await context.Sections
                .FirstOrDefaultAsync(s => s.Id == enrollment.SectionId, cancellationToken);
            sectionName = section?.Name ?? "—";

            var grade = await context.GradeLevels
                .FirstOrDefaultAsync(g => g.Id == enrollment.GradeLevelId, cancellationToken);
            gradeLevel = grade?.Name ?? "—";
        }

        // Get class offerings for the student's section
        List<long> offeringIds = [];
        Dictionary<long, (long SubjectId, long? TeacherId, string? Room)> offeringMap = [];

        if (enrollment is not null)
        {
            var offerings = await context.ClassOfferings
                .Where(co => co.SectionId == enrollment.SectionId)
                .Where(co => co.Status == "ACTIVE")
                .Select(co => new { co.Id, co.SubjectId, co.TeacherId, co.Room })
                .ToListAsync(cancellationToken);

            offeringIds = offerings.Select(o => o.Id).ToList();
            offeringMap = offerings.ToDictionary(o => o.Id, o => (o.SubjectId, o.TeacherId, o.Room));
        }

        // Get subjects and teachers for display
        Dictionary<long, string> subjectMap = [];
        Dictionary<long, string> teacherMap = [];

        if (offeringIds.Count > 0)
        {
            var subjectIds = offeringMap.Values.Select(v => v.SubjectId).Distinct().ToList();
            var teacherIds = offeringMap.Values
                .Where(v => v.TeacherId.HasValue)
                .Select(v => v.TeacherId!.Value)
                .Distinct()
                .ToList();

            var subjects = await context.Subjects
                .Where(s => subjectIds.Contains(s.Id))
                .ToListAsync(cancellationToken);
            subjectMap = subjects.ToDictionary(s => s.Id, s => s.Title);

            if (teacherIds.Count > 0)
            {
                var teachers = await context.Teachers
                    .Where(t => teacherIds.Contains(t.Id))
                    .ToListAsync(cancellationToken);
                teacherMap = teachers.ToDictionary(t => t.Id, t => $"{t.FirstName} {t.LastName}");
            }
        }

        // Build weekly schedule
        var today = DateTime.Now;
        var todayDbDay = (byte)(((int)today.DayOfWeek + 6) % 7 + 1);

        List<StudentWeeklyScheduleView> weeklySchedule = [];

        if (offeringIds.Count > 0)
        {
            var schedulesRaw = await context.ClassSchedules
                .Where(cs => offeringIds.Contains(cs.ClassOfferingId))
                .OrderBy(cs => cs.DayOfWeek)
                .ThenBy(cs => cs.StartTime)
                .Select(cs => new { cs.ClassOfferingId, cs.DayOfWeek, cs.StartTime, cs.EndTime })
                .ToListAsync(cancellationToken);

            weeklySchedule = schedulesRaw
                .Select(sr =>
                {
                    var off = offeringMap.GetValueOrDefault(sr.ClassOfferingId);
                    var subjectId = off.SubjectId;
                    var teacherId = off.TeacherId;
                    return new StudentWeeklyScheduleView
                    {
                        Subject = subjectMap.GetValueOrDefault(subjectId, "—"),
                        Teacher = teacherId.HasValue ? teacherMap.GetValueOrDefault(teacherId.Value, "—") : "—",
                        Day = DayNames[sr.DayOfWeek],
                        StartTime = sr.StartTime.ToString("h:mm tt", CultureInfo.InvariantCulture),
                        EndTime = sr.EndTime.ToString("h:mm tt", CultureInfo.InvariantCulture),
                        Room = off.Room
                    };
                })
                .ToList();
        }

        var todaySchedule = weeklySchedule
            .Where(s => s.Day == DayNames[todayDbDay])
            .ToList();

        // Get attendance records
        var attendanceRecords = await context.AttendanceRecords
            .Where(r => r.StudentId == studentId)
            .Include(r => r.Session)
                .ThenInclude(s => s!.ClassOffering)
                    .ThenInclude(co => co!.Subject)
            .Include(r => r.Session)
                .ThenInclude(s => s!.ClassOffering)
                    .ThenInclude(co => co!.Teacher)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var totalSessions = attendanceRecords.Count;
        var presentCount = attendanceRecords.Count(r => r.Status == "PRESENT");
        var lateCount = attendanceRecords.Count(r => r.Status == "LATE");
        var absentCount = attendanceRecords.Count(r => r.Status == "ABSENT");
        var attendanceRate = totalSessions > 0
            ? Math.Round((double)(presentCount + lateCount) / totalSessions * 100, 1)
            : 0;

        var recentAttendance = attendanceRecords
            .Take(20)
            .Select(r => new StudentAttendanceHistoryView
            {
                Date = r.Session?.SessionDate ?? r.CreatedAt,
                Subject = r.Session?.SubjectName
                    ?? r.Session?.ClassOffering?.Subject?.Title
                    ?? "—",
                Status = r.Status,
                Teacher = r.Session?.ClassOffering?.Teacher is not null
                    ? $"{r.Session.ClassOffering.Teacher.FirstName} {r.Session.ClassOffering.Teacher.LastName}"
                    : null,
                Reason = r.Reason
            })
            .ToList();

        return new StudentDashboardData
        {
            StudentId = student.Id,
            FullName = student.FullName,
            Lrn = student.Lrn,
            StudentNumber = student.StudentNumber,
            ProfilePicture = student.ProfilePicture,
            SectionName = sectionName,
            GradeLevel = gradeLevel,
            CurrentSchoolYear = schoolYearName,
            TotalSessions = totalSessions,
            PresentCount = presentCount,
            LateCount = lateCount,
            AbsentCount = absentCount,
            AttendanceRate = attendanceRate,
            WeeklySchedule = weeklySchedule,
            TodaySchedule = todaySchedule,
            RecentAttendance = recentAttendance
        };
    }

    public async Task<List<StudentAttendanceHistoryView>> GetAttendanceHistoryAsync(long studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var records = await context.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.StudentId == studentId)
            .Include(r => r.Session)
                .ThenInclude(s => s!.ClassOffering)
                    .ThenInclude(co => co!.Subject)
            .Include(r => r.Session)
                .ThenInclude(s => s!.ClassOffering)
                    .ThenInclude(co => co!.Teacher)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return records
            .Select(r => new StudentAttendanceHistoryView
            {
                Date = r.Session?.SessionDate ?? r.CreatedAt,
                Subject = r.Session?.SubjectName
                    ?? r.Session?.ClassOffering?.Subject?.Title
                    ?? "—",
                Status = r.Status,
                Teacher = r.Session?.ClassOffering?.Teacher is not null
                    ? $"{r.Session.ClassOffering.Teacher.FirstName} {r.Session.ClassOffering.Teacher.LastName}"
                    : null,
                Reason = r.Reason
            })
            .ToList();
    }

    public async Task<bool> UpdateProfilePictureAsync(long studentId, byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var student = await context.Students.FindAsync([studentId], cancellationToken);
        if (student is null) return false;

        student.ProfilePicture = imageBytes;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
