using AttendanceUI.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

/// <summary>Which students a teacher is responsible for: those in sections they teach or advise this school year.</summary>
internal static class TeacherScope
{
    public static async Task<List<long>> GetStudentIdsAsync(SchoolDbContext context, long teacherUserId, CancellationToken ct)
    {
        var teacherId = await context.Teachers
            .Where(t => t.UserId == teacherUserId)
            .Select(t => (long?)t.Id)
            .FirstOrDefaultAsync(ct);
        if (teacherId is null) return [];

        var activeYear = await context.SchoolYears
            .Where(y => y.Status == "ACTIVE")
            .Select(y => y.Id)
            .FirstOrDefaultAsync(ct);

        var taught = await context.ClassOfferings
            .Where(o => o.TeacherId == teacherId && o.SchoolYearId == activeYear)
            .Select(o => o.SectionId)
            .ToListAsync(ct);
        var advised = await context.Sections
            .Where(s => s.AdviserTeacherId == teacherId)
            .Select(s => s.Id)
            .ToListAsync(ct);
        var sectionIds = taught.Concat(advised).Distinct().ToList();

        return await context.Enrollments
            .Where(e => sectionIds.Contains(e.SectionId) && e.SchoolYearId == activeYear && e.Status == "ACTIVE")
            .Select(e => e.StudentId)
            .Distinct()
            .ToListAsync(ct);
    }
}
