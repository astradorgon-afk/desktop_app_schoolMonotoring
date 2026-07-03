using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class TeacherService : ITeacherService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private static readonly string[] DayNames = ["", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    public TeacherService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task<IReadOnlyList<Teacher>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Teachers
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Teacher?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Teachers
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);
    }

    public async Task<TeacherDashboardData?> GetTeacherDashboardAsync(long teacherId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var teacher = await context.Teachers
            .FirstOrDefaultAsync(t => t.Id == teacherId, cancellationToken);

        if (teacher is null) return null;

        var today = DateTime.Now;
        var todayDbDay = (byte)(((int)today.DayOfWeek + 6) % 7 + 1);
        var todayDate = DateOnly.FromDateTime(today);

        var offerings = await context.ClassOfferings
            .Where(co => co.TeacherId == teacherId)
            .Select(co => new { co.Id, co.SectionId, co.SubjectId, co.Room })
            .ToListAsync(cancellationToken);

        if (offerings.Count == 0)
        {
            return new TeacherDashboardData
            {
                Teacher = teacher,
                TodaySchedule = [],
                WeeklySchedule = []
            };
        }

        var offeringIds = offerings.Select(o => o.Id).ToList();
        var sectionIds = offerings.Select(o => o.SectionId).Distinct().ToList();
        var subjectIds = offerings.Select(o => o.SubjectId).Distinct().ToList();

        var sections = await context.Sections
            .Where(s => sectionIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        var subjects = await context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Title, cancellationToken);

        var schedulesRaw = await context.ClassSchedules
            .Where(cs => offeringIds.Contains(cs.ClassOfferingId))
            .Select(cs => new { cs.ClassOfferingId, cs.DayOfWeek, cs.StartTime, cs.EndTime })
            .ToListAsync(cancellationToken);

        var offeringMap = offerings.ToDictionary(o => o.Id);

        var dayOrder = new Dictionary<string, int>
        {
            ["Monday"] = 1, ["Tuesday"] = 2, ["Wednesday"] = 3,
            ["Thursday"] = 4, ["Friday"] = 5, ["Saturday"] = 6, ["Sunday"] = 7
        };

        var allSchedules = schedulesRaw
            .Select(sr =>
            {
                var off = offeringMap.GetValueOrDefault(sr.ClassOfferingId);
                return new TeacherScheduleView
                {
                    Section = off is not null ? sections.GetValueOrDefault(off.SectionId, "—") : "—",
                    Subject = off is not null ? subjects.GetValueOrDefault(off.SubjectId, "—") : "—",
                    Day = DayNames[sr.DayOfWeek],
                    StartTime = sr.StartTime.ToString(),
                    EndTime = sr.EndTime.ToString(),
                    Room = off?.Room
                };
            })
            .ToList();

        var todaySchedule = allSchedules
            .Where(s => s.Day == DayNames[todayDbDay])
            .OrderBy(s => s.StartTime)
            .ToList();

        var weeklySchedule = allSchedules
            .OrderBy(s => dayOrder.GetValueOrDefault(s.Day, 99))
            .ThenBy(s => s.StartTime)
            .ToList();

        var todayOfferingIds = schedulesRaw
            .Where(cs => cs.DayOfWeek == todayDbDay)
            .Select(cs => cs.ClassOfferingId)
            .Distinct()
            .ToList();

        var activeSession = await context.AttendanceSessions
            .Where(s => s.SessionDate == todayDate.ToDateTime(TimeOnly.MinValue))
            .Where(s => todayOfferingIds.Contains(s.ClassOfferingId))
            .Select(s => new { s.Id, s.ClassOfferingId })
            .FirstOrDefaultAsync(cancellationToken);

        var totalStudents = await context.Enrollments
            .Where(e => e.Status == "ACTIVE")
            .Where(e => context.ClassOfferings
                .Where(co => co.TeacherId == teacherId)
                .Select(co => co.SectionId)
                .Contains(e.SectionId))
            .CountAsync(cancellationToken);

        int presentToday = 0, lateToday = 0, absentToday = 0;

        if (activeSession is not null)
        {
            var records = await context.AttendanceRecords
                .Where(r => r.AttendanceSessionId == activeSession.Id)
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            foreach (var r in records)
            {
                switch (r.Status)
                {
                    case "PRESENT": presentToday = r.Count; break;
                    case "LATE": lateToday = r.Count; break;
                    case "ABSENT": absentToday = r.Count; break;
                }
            }
        }

        return new TeacherDashboardData
        {
            Teacher = teacher,
            TotalClassesToday = todaySchedule.Count,
            TotalStudents = totalStudents,
            PresentToday = presentToday,
            LateToday = lateToday,
            AbsentToday = absentToday,
            TodaySchedule = todaySchedule,
            WeeklySchedule = weeklySchedule
        };
    }

    public async Task<List<TeacherSectionView>> GetHandledSectionsAsync(long teacherId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var offerings = await context.ClassOfferings
            .Where(co => co.TeacherId == teacherId)
            .Select(co => new { co.Id, co.SectionId, co.SubjectId, co.Section!.Name, co.Section.GradeLevelId, co.Room })
            .ToListAsync(cancellationToken);

        if (offerings.Count == 0)
            return [];

        var sectionIds = offerings.Select(o => o.SectionId).Distinct().ToList();
        var subjectIds = offerings.Select(o => o.SubjectId).Distinct().ToList();
        var gradeLevelIds = offerings.Select(o => o.GradeLevelId).Distinct().ToList();

        var gradeLevels = await context.GradeLevels
            .Where(g => gradeLevelIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, g => g.Name, cancellationToken);

        var subjects = await context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Title, cancellationToken);

        var activeSchoolYear = await context.SchoolYears
            .Where(sy => sy.Status == "ACTIVE")
            .Select(sy => sy.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var studentCounts = await context.Enrollments
            .Where(e => e.Status == "ACTIVE")
            .Where(e => sectionIds.Contains(e.SectionId))
            .Where(e => e.SchoolYearId == activeSchoolYear)
            .GroupBy(e => e.SectionId)
            .Select(g => new { SectionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.SectionId, g => g.Count, cancellationToken);

        var offeringIds = offerings.Select(o => o.Id).ToList();
        var schedulesRaw = await context.ClassSchedules
            .Where(cs => offeringIds.Contains(cs.ClassOfferingId))
            .Select(cs => new { cs.ClassOfferingId, cs.DayOfWeek, cs.StartTime, cs.EndTime })
            .ToListAsync(cancellationToken);

        var offeringMap = offerings.ToDictionary(o => o.Id);
        var dayOrder = new Dictionary<string, int>
        {
            ["Monday"] = 1, ["Tuesday"] = 2, ["Wednesday"] = 3,
            ["Thursday"] = 4, ["Friday"] = 5, ["Saturday"] = 6, ["Sunday"] = 7
        };

        var sectionGroups = offerings
            .GroupBy(o => o.SectionId)
            .ToList();

        var result = new List<TeacherSectionView>();

        foreach (var group in sectionGroups)
        {
            var first = group.First();
            var sectionOfferIds = group.Select(o => o.Id).ToHashSet();

            var subjectNames = group
                .Select(o => subjects.GetValueOrDefault(o.SubjectId, "—"))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            var sectionSchedules = schedulesRaw
                .Where(sr => sectionOfferIds.Contains(sr.ClassOfferingId))
                .Select(sr =>
                {
                    var off = offeringMap.GetValueOrDefault(sr.ClassOfferingId);
                    return new TeacherScheduleView
                    {
                        Section = first.Name,
                        Subject = off is not null ? subjects.GetValueOrDefault(off.SubjectId, "—") : "—",
                        Day = DayNames[sr.DayOfWeek],
                        StartTime = sr.StartTime.ToString(),
                        EndTime = sr.EndTime.ToString(),
                        Room = off?.Room
                    };
                })
                .OrderBy(s => dayOrder.GetValueOrDefault(s.Day, 99))
                .ThenBy(s => s.StartTime)
                .ToList();

            result.Add(new TeacherSectionView
            {
                SectionId = first.SectionId,
                SectionName = first.Name,
                GradeLevel = gradeLevels.GetValueOrDefault(first.GradeLevelId, "—"),
                StudentCount = studentCounts.GetValueOrDefault(first.SectionId, 0),
                Subjects = subjectNames,
                Schedules = sectionSchedules
            });
        }

        return result.OrderBy(s => s.GradeLevel).ThenBy(s => s.SectionName).ToList();
    }

    public async Task<List<SectionEnrolledStudent>> GetSectionStudentsAsync(long sectionId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var activeSchoolYear = await context.SchoolYears
            .Where(sy => sy.Status == "ACTIVE")
            .Select(sy => sy.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return await context.Enrollments
            .Where(e => e.SectionId == sectionId)
            .Where(e => e.Status == "ACTIVE")
            .Where(e => e.SchoolYearId == activeSchoolYear)
            .Join(context.Students,
                e => e.StudentId,
                s => s.Id,
                (e, s) => new SectionEnrolledStudent
                {
                    StudentId = s.Id,
                    FullName = s.LastName + ", " + s.FirstName + (s.MiddleName != null ? " " + s.MiddleName : ""),
                    Lrn = s.Lrn,
                    StudentNumber = s.StudentNumber
                })
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<TeacherDetail?> GetDetailAsync(long teacherId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var teacher = await context.Teachers
            .FirstOrDefaultAsync(t => t.Id == teacherId, cancellationToken);

        if (teacher is null) return null;

        var offerings = await context.ClassOfferings
            .Where(co => co.TeacherId == teacherId)
            .Select(co => new { co.Id, co.SectionId, co.SubjectId, co.Room })
            .ToListAsync(cancellationToken);

        var schedules = new List<TeacherScheduleView>();

        if (offerings.Count > 0)
        {
            var offeringIds = offerings.Select(o => o.Id).ToList();
            var sectionIds = offerings.Select(o => o.SectionId).Distinct().ToList();
            var subjectIds = offerings.Select(o => o.SubjectId).Distinct().ToList();

            var sections = await context.Sections
                .Where(s => sectionIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

            var subjects = await context.Subjects
                .Where(s => subjectIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Title, cancellationToken);

            var schedulesRaw = await context.ClassSchedules
                .Where(cs => offeringIds.Contains(cs.ClassOfferingId))
                .Select(cs => new { cs.ClassOfferingId, cs.DayOfWeek, cs.StartTime, cs.EndTime })
                .ToListAsync(cancellationToken);

            var offeringMap = offerings.ToDictionary(o => o.Id);

            var dayOrder = new Dictionary<string, int>
            {
                ["Monday"] = 1, ["Tuesday"] = 2, ["Wednesday"] = 3,
                ["Thursday"] = 4, ["Friday"] = 5, ["Saturday"] = 6, ["Sunday"] = 7
            };

            schedules = schedulesRaw
                .Select(sr =>
                {
                    var off = offeringMap.GetValueOrDefault(sr.ClassOfferingId);
                    return new TeacherScheduleView
                    {
                        Section = off is not null ? sections.GetValueOrDefault(off.SectionId, "—") : "—",
                        Subject = off is not null ? subjects.GetValueOrDefault(off.SubjectId, "—") : "—",
                        Day = DayNames[sr.DayOfWeek],
                        StartTime = sr.StartTime.ToString(),
                        EndTime = sr.EndTime.ToString(),
                        Room = off?.Room
                    };
                })
                .OrderBy(s => dayOrder.GetValueOrDefault(s.Day, 99))
                .ThenBy(s => s.StartTime)
                .ToList();
        }

        return new TeacherDetail
        {
            Teacher = teacher,
            Schedules = schedules
        };
    }
}
