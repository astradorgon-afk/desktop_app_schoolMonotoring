using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

public class ScheduleService : IScheduleService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    public ScheduleService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    public async Task<SchedulePageData> GetPageDataAsync(
        string? gradeLevel = null, string? status = null, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var gradeFilter = gradeLevel switch
        {
            "G7" => 1, "G8" => 2, "G9" => 3, "G10" => 4,
            _ => (long?)null
        };

        var sectionQuery = context.Sections
            .Where(s => !s.IsArchived);

        if (gradeFilter.HasValue)
            sectionQuery = sectionQuery.Where(s => s.GradeLevelId == gradeFilter.Value);

        var sections = await sectionQuery
            .Select(s => new { s.Id, s.Name, s.GradeLevelId })
            .ToListAsync(ct);

        var gradeLevels = await context.GradeLevels
            .ToDictionaryAsync(gl => gl.Id, gl => gl.Name, ct);

        var enrollmentCounts = await context.Enrollments
            .Where(e => e.Status == "ENROLLED")
            .GroupBy(e => e.SectionId)
            .Select(g => new { SectionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(e => e.SectionId, e => e.Count, ct);

        var sectionCards = sections.Select(s => new SectionCardInfo
        {
            SectionId = s.Id,
            SectionName = s.Name,
            GradeLevel = gradeLevels.GetValueOrDefault(s.GradeLevelId, ""),
            GradeLevelId = s.GradeLevelId,
            StudentCount = enrollmentCounts.GetValueOrDefault(s.Id, 0)
        }).ToList();

        var offeringQuery = context.ClassOfferings.AsQueryable();

        if (gradeFilter.HasValue)
        {
            var sectionIds = sections.Select(s => s.Id).ToList();
            offeringQuery = offeringQuery.Where(o => sectionIds.Contains(o.SectionId));
        }

        if (!string.IsNullOrEmpty(status))
            offeringQuery = offeringQuery.Where(o => o.Status == status);

        var offerings = await offeringQuery
            .Select(o => new { o.Id, o.SectionId, o.SubjectId, o.TeacherId, o.Room, o.Status })
            .ToListAsync(ct);

        var offeringIds = offerings.Select(o => o.Id).ToList();
        var subjectIds = offerings.Select(o => o.SubjectId).Distinct().ToList();
        var teacherIds = offerings.Select(o => o.TeacherId).Where(t => t.HasValue).Select(t => t!.Value).Distinct().ToList();
        var sectionIds2 = offerings.Select(o => o.SectionId).Distinct().ToList();

        var subjects = await context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Title, ct);

        var teachers = await context.Teachers
            .Where(t => teacherIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => $"{t.LastName}, {t.FirstName}", ct);

        var sectionMap = sections.ToDictionary(s => s.Id);

        var schedulesRaw = await context.ClassSchedules
            .Where(cs => offeringIds.Contains(cs.ClassOfferingId))
            .Select(cs => new { cs.Id, cs.ClassOfferingId, cs.DayOfWeek, cs.StartTime, cs.EndTime })
            .ToListAsync(ct);

        var offeringMap = offerings.ToDictionary(o => o.Id);

        var entries = schedulesRaw.Select(sr =>
        {
            var off = offeringMap.GetValueOrDefault(sr.ClassOfferingId);
            var sec = off is not null ? sectionMap.GetValueOrDefault(off.SectionId) : null;
            return new ScheduleViewEntry
            {
                Id = sr.Id,
                OfferingId = sr.ClassOfferingId,
                DayOfWeek = sr.DayOfWeek,
                StartTime = sr.StartTime,
                EndTime = sr.EndTime,
                SectionName = sec?.Name ?? "—",
                GradeLevel = sec is not null ? gradeLevels.GetValueOrDefault(sec.GradeLevelId, "") : "",
                Subject = off is not null ? subjects.GetValueOrDefault(off.SubjectId, "—") : "—",
                TeacherName = off?.TeacherId is not null ? teachers.GetValueOrDefault(off.TeacherId.Value, "—") : "—",
                Room = off?.Room,
                OfferingStatus = off?.Status ?? "",
                SectionId = off?.SectionId ?? 0,
                SubjectId = off?.SubjectId ?? 0,
                TeacherId = off?.TeacherId
            };
        }).ToList();

        return new SchedulePageData
        {
            Sections = sectionCards,
            Entries = entries
        };
    }

    public async Task<List<Subject>> GetSubjectsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Subjects
            .Where(s => s.IsActive)
            .OrderBy(s => s.Title)
            .ToListAsync(ct);
    }

    public async Task<List<Teacher>> GetTeachersAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Teachers
            .Where(t => t.Status == "ACTIVE")
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync(ct);
    }

    public async Task<List<Section>> GetSectionsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Sections
            .Where(s => !s.IsArchived)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<List<Room>> GetRoomsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Rooms
            .Where(r => r.IsActive)
            .OrderBy(r => r.Code)
            .ToListAsync(ct);
    }

    private static async Task CheckConflictsAsync(SchoolDbContext context,
        long sectionId, long teacherId, string? room,
        byte dayOfWeek, TimeOnly startTime, TimeOnly endTime,
        long? excludeScheduleId = null, CancellationToken ct = default)
    {
        // Overlapping schedules on the same day
        IQueryable<ClassSchedule> overlapQuery = context.ClassSchedules
            .Where(cs => cs.DayOfWeek == dayOfWeek
                      && cs.StartTime < endTime
                      && cs.EndTime > startTime);

        if (excludeScheduleId.HasValue)
            overlapQuery = overlapQuery.Where(cs => cs.Id != excludeScheduleId.Value);

        var overlappingScheduleIds = await overlapQuery
            .Select(cs => cs.ClassOfferingId)
            .Distinct()
            .ToListAsync(ct);

        if (overlappingScheduleIds.Count == 0) return;

        // Teacher conflict
        var teacherConflict = await context.ClassOfferings
            .AnyAsync(o => overlappingScheduleIds.Contains(o.Id)
                        && o.TeacherId == teacherId, ct);

        if (teacherConflict)
            throw new InvalidOperationException("Conflict detected");

        // Section conflict
        var sectionConflict = await context.ClassOfferings
            .AnyAsync(o => overlappingScheduleIds.Contains(o.Id)
                        && o.SectionId == sectionId, ct);

        if (sectionConflict)
            throw new InvalidOperationException("Conflict detected");

        // Room conflict
        if (!string.IsNullOrEmpty(room))
        {
            var roomConflict = await context.ClassOfferings
                .AnyAsync(o => overlappingScheduleIds.Contains(o.Id)
                            && o.Room == room, ct);

            if (roomConflict)
                throw new InvalidOperationException("Conflict detected");
        }
    }

    public async Task AddScheduleEntryAsync(long sectionId, long subjectId, long teacherId,
        byte dayOfWeek, TimeOnly startTime, TimeOnly endTime, string? room, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var schoolYear = await context.SchoolYears
            .Where(sy => sy.Status == "ACTIVE")
            .FirstOrDefaultAsync(ct);

        if (schoolYear is null)
            throw new InvalidOperationException("No active school year found.");

        // Check for duplicate offering (same section + subject in same school year)
        var existing = await context.ClassOfferings
            .Where(o => o.SchoolYearId == schoolYear.Id
                     && o.SectionId == sectionId
                     && o.SubjectId == subjectId)
            .FirstOrDefaultAsync(ct);

        if (existing is not null)
        {
            // Reuse existing offering — check conflicts then add schedule
            await CheckConflictsAsync(context, sectionId, teacherId, room,
                dayOfWeek, startTime, endTime, null, ct);

            var schedule = new ClassSchedule
            {
                ClassOfferingId = existing.Id,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ClassSchedules.Add(schedule);
            await context.SaveChangesAsync(ct);
            return;
        }

        // Check all conflicts before creating
        await CheckConflictsAsync(context, sectionId, teacherId, room,
            dayOfWeek, startTime, endTime, null, ct);

        var offering = new ClassOffering
        {
            SchoolYearId = schoolYear.Id,
            SectionId = sectionId,
            SubjectId = subjectId,
            TeacherId = teacherId,
            Room = room,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.ClassOfferings.Add(offering);
        await context.SaveChangesAsync(ct);

        var scheduleEntry = new ClassSchedule
        {
            ClassOfferingId = offering.Id,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.ClassSchedules.Add(scheduleEntry);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateScheduleEntryAsync(long offeringId, long scheduleId, long subjectId, long teacherId,
        byte dayOfWeek, TimeOnly startTime, TimeOnly endTime, string? room, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var offering = await context.ClassOfferings
            .FirstOrDefaultAsync(o => o.Id == offeringId, ct);
        if (offering is null)
            throw new InvalidOperationException("Class offering not found.");

        var schedule = await context.ClassSchedules
            .FirstOrDefaultAsync(cs => cs.Id == scheduleId && cs.ClassOfferingId == offeringId, ct);
        if (schedule is null)
            throw new InvalidOperationException("Schedule entry not found.");

        // Check conflicts, excluding this schedule from the overlap check
        await CheckConflictsAsync(context, offering.SectionId, teacherId, room,
            dayOfWeek, startTime, endTime, scheduleId, ct);

        offering.SubjectId = subjectId;
        offering.TeacherId = teacherId;
        offering.Room = room;
        offering.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        schedule.DayOfWeek = dayOfWeek;
        schedule.StartTime = startTime;
        schedule.EndTime = endTime;
        schedule.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteScheduleEntryAsync(long scheduleId, long offeringId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var schedule = await context.ClassSchedules
            .FirstOrDefaultAsync(cs => cs.Id == scheduleId, ct);
        if (schedule is not null)
        {
            context.ClassSchedules.Remove(schedule);
            await context.SaveChangesAsync(ct);
        }

        // Check if offering has any remaining schedules
        var remaining = await context.ClassSchedules
            .AnyAsync(cs => cs.ClassOfferingId == offeringId, ct);

        if (!remaining)
        {
            var offering = await context.ClassOfferings
                .FirstOrDefaultAsync(o => o.Id == offeringId, ct);
            if (offering is not null)
            {
                context.ClassOfferings.Remove(offering);
                await context.SaveChangesAsync(ct);
            }
        }
    }
}
