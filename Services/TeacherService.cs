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
