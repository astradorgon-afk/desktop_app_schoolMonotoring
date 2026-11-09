using System.Globalization;
using System.Text;
using AttendanceUI.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceUI.Services;

/// <summary>
/// Produces CSV or Excel exports of the core school records (students, teachers, sections)
/// and of attendance (per teacher session, per student record, and per-student totals) for
/// administrators. Each dataset is first built as an in-memory table (headers + rows) which
/// is shared by both the on-screen preview and the file, so the two can never drift apart.
/// CSV bytes are UTF-8 (BOM) so they open cleanly in Excel; Excel output is a real .xlsx
/// workbook (one sheet per dataset) written by <see cref="XlsxWriter"/>.
/// </summary>
public class ExportService : IExportService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;

    // How many rows a preview shows before truncating (the CSV always has them all).
    private const int PreviewRowLimit = 100;

    public ExportService(IDbContextFactory<SchoolDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// An in-memory representation of an export: file kind, a display title, its rows, and
    /// which columns hold numbers (so Excel stores them as numbers rather than text).
    /// </summary>
    private sealed record ExportTable(
        string Kind,
        string Title,
        IReadOnlyList<string> Headers,
        IReadOnlyList<string?[]> Rows,
        IReadOnlySet<int>? NumericColumns = null);

    /// <summary>Attendance statuses accepted by <see cref="ExportOptions.Status"/>.</summary>
    private static readonly string[] KnownStatuses = ["PRESENT", "LATE", "ABSENT", "EXCUSED"];

    // Datasets included in "Export All", in the order they are produced.
    private static readonly ExportDataset[] AllDatasets =
    [
        ExportDataset.Students,
        ExportDataset.Teachers,
        ExportDataset.Sections,
        ExportDataset.TeacherSessions,
        ExportDataset.StudentAttendance,
        ExportDataset.AttendanceSummary
    ];

    public async Task<ExportFile> ExportAsync(ExportDataset dataset, ExportOptions? options = null,
        CancellationToken cancellationToken = default, ExportFormat format = ExportFormat.Csv)
    {
        var table = await BuildTableAsync(dataset, options ?? ExportOptions.None, cancellationToken);
        return format == ExportFormat.Excel
            ? ToWorkbook(table.Kind, [table])
            : ToFile(table);
    }

    public async Task<IReadOnlyList<ExportFile>> ExportAllAsync(ExportOptions? options = null,
        CancellationToken cancellationToken = default, ExportFormat format = ExportFormat.Csv)
    {
        var opts = options ?? ExportOptions.None;
        var tables = new List<ExportTable>(AllDatasets.Length);

        foreach (var dataset in AllDatasets)
            tables.Add(await BuildTableAsync(dataset, opts, cancellationToken));

        if (format == ExportFormat.Excel)
        {
            var range = DescribeRange(opts);
            return [ToWorkbook(range is null ? "eduattend-export" : $"eduattend-export-{Slug(range)}", tables)];
        }

        return tables.Select(ToFile).ToList();
    }

    public async Task<ExportPreview> PreviewAsync(ExportDataset dataset, ExportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var table = await BuildTableAsync(dataset, options ?? ExportOptions.None, cancellationToken);
        return ToPreview(dataset, table);
    }

    public async Task<IReadOnlyList<ExportPreview>> PreviewAllAsync(ExportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var opts = options ?? ExportOptions.None;
        var previews = new List<ExportPreview>(AllDatasets.Length);

        foreach (var dataset in AllDatasets)
            previews.Add(ToPreview(dataset, await BuildTableAsync(dataset, opts, cancellationToken)));

        return previews;
    }

    public async Task<IReadOnlyList<TeacherOption>> GetTeacherOptionsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Only teachers attached to a class offering can appear in the sessions export, so
        // those are the only ones worth offering as a filter. Expressed as a correlated
        // EXISTS rather than a client-side list + Contains, which Pomelo cannot translate.
        var teachers = await context.Teachers
            .AsNoTracking()
            .Where(t => context.ClassOfferings.Any(o => o.TeacherId == t.Id))
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Select(t => new { t.Id, t.LastName, t.FirstName, t.MiddleName, t.EmployeeNo })
            .ToListAsync(cancellationToken);

        return teachers
            .Select(t => new TeacherOption(t.Id, FormatName(t.LastName, t.FirstName, t.MiddleName), t.EmployeeNo))
            .ToList();
    }

    public async Task<IReadOnlyList<SectionOption>> GetSectionOptionsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var sections = await (
            from sec in context.Sections.AsNoTracking()
            where !sec.IsArchived
            join gl in context.GradeLevels.AsNoTracking() on sec.GradeLevelId equals gl.Id into glJoin
            from gl in glJoin.DefaultIfEmpty()
            orderby gl != null ? gl.Name : "", sec.Name
            select new { sec.Id, sec.Name, GradeName = gl != null ? gl.Name : null })
            .ToListAsync(cancellationToken);

        return sections
            .Select(s => new SectionOption(s.Id, s.Name, s.GradeName))
            .ToList();
    }

    public async Task<IReadOnlyList<StudentOption>> GetStudentOptionsAsync(long? sectionId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Only students with at least one attendance record can appear in the export.
        var query = context.Students
            .AsNoTracking()
            .Where(s => context.AttendanceRecords.Any(r => r.StudentId == s.Id));

        if (sectionId is { } id)
            query = query.Where(s => context.Enrollments.Any(e => e.StudentId == s.Id && e.SectionId == id));

        var students = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Select(s => new { s.Id, s.LastName, s.FirstName, s.MiddleName, s.Lrn })
            .ToListAsync(cancellationToken);

        return students
            .Select(s => new StudentOption(s.Id, FormatName(s.LastName, s.FirstName, s.MiddleName), s.Lrn))
            .ToList();
    }

    private static ExportPreview ToPreview(ExportDataset dataset, ExportTable table)
    {
        var rows = table.Rows.Count > PreviewRowLimit
            ? table.Rows.Take(PreviewRowLimit).ToList()
            : table.Rows;

        return new ExportPreview(table.Title, table.Headers, rows, table.Rows.Count, dataset);
    }

    private Task<ExportTable> BuildTableAsync(ExportDataset dataset, ExportOptions options, CancellationToken ct) =>
        dataset switch
        {
            ExportDataset.Students => BuildStudentsAsync(ct),
            ExportDataset.Teachers => BuildTeachersAsync(ct),
            ExportDataset.Sections => BuildSectionsAsync(ct),
            ExportDataset.TeacherSessions => BuildTeacherSessionsAsync(options, ct),
            ExportDataset.StudentAttendance => BuildStudentAttendanceAsync(options, ct),
            ExportDataset.AttendanceSummary => BuildAttendanceSummaryAsync(options, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(dataset), dataset, "Unknown export dataset.")
        };

    private async Task<ExportTable> BuildStudentsAsync(CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var students = await context.Students
            .AsNoTracking()
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(ct);

        // Map each student to their current section / grade level (if enrolled).
        var placementByStudent = await LoadPlacementsAsync(context, ct);

        string[] headers =
        [
            "LRN", "Student No", "Last Name", "First Name", "Middle Name", "Sex",
            "Birthdate", "Age", "Contact No", "Address", "Guardian Name", "Guardian Contact",
            "Previous School", "Grade Level", "Section", "Status"
        ];

        var rows = new List<string?[]>(students.Count);
        foreach (var s in students)
        {
            placementByStudent.TryGetValue(s.Id, out var placement);
            rows.Add(
            [
                s.Lrn,
                s.StudentNumber,
                s.LastName,
                s.FirstName,
                s.MiddleName,
                s.Sex,
                s.Birthdate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                s.Age?.ToString(CultureInfo.InvariantCulture),
                s.ContactNo,
                s.Address,
                s.GuardianName,
                s.GuardianContact,
                s.PreviousSchool,
                placement?.GradeName,
                placement?.SectionName,
                s.Status
            ]);
        }

        return new ExportTable("students", "Students", headers, rows, new HashSet<int> { 7 });
    }

    /// <summary>
    /// Each student's current grade level and section, from their active enrolment in the
    /// active school year (any year when none is marked active).
    /// </summary>
    private static async Task<Dictionary<long, Placement>> LoadPlacementsAsync(SchoolDbContext context, CancellationToken ct)
    {
        var activeYearId = await context.SchoolYears
            .Where(y => y.Status == "ACTIVE")
            .Select(y => y.Id)
            .FirstOrDefaultAsync(ct);

        // NOTE: use an explicit OR predicate rather than array.Contains(...).
        // Pomelo/EF Core 9 cannot translate a parameterized primitive-collection
        // Contains here and throws "An exception was thrown while attempting to
        // evaluate a LINQ query parameter expression."
        var enrollmentQuery = context.Enrollments
            .AsNoTracking()
            .Where(e => e.Status == "ACTIVE" || e.Status == "ENROLLED");

        if (activeYearId != 0)
            enrollmentQuery = enrollmentQuery.Where(e => e.SchoolYearId == activeYearId);

        var placements = await (
            from e in enrollmentQuery
            join sec in context.Sections on e.SectionId equals sec.Id into secJoin
            from sec in secJoin.DefaultIfEmpty()
            join gl in context.GradeLevels on e.GradeLevelId equals gl.Id into glJoin
            from gl in glJoin.DefaultIfEmpty()
            select new
            {
                e.StudentId,
                SectionName = sec != null ? sec.Name : null,
                GradeName = gl != null ? gl.Name : null
            }).ToListAsync(ct);

        return placements
            .GroupBy(p => p.StudentId)
            .ToDictionary(g => g.Key, g => new Placement(g.First().GradeName, g.First().SectionName));
    }

    private sealed record Placement(string? GradeName, string? SectionName);

    private async Task<ExportTable> BuildTeachersAsync(CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var teachers = await context.Teachers
            .AsNoTracking()
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync(ct);

        string[] headers =
        [
            "Employee No", "Last Name", "First Name", "Middle Name", "Email", "Contact No",
            "Specialization", "Employment Status", "Advisory Assignment", "Hire Date", "Status"
        ];

        var rows = new List<string?[]>(teachers.Count);
        foreach (var t in teachers)
        {
            rows.Add(
            [
                t.EmployeeNo,
                t.LastName,
                t.FirstName,
                t.MiddleName,
                t.Email,
                t.ContactNo,
                t.Specialization,
                t.EmploymentStatus,
                t.AdvisoryAssignmentStatus,
                t.HireDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                t.Status
            ]);
        }

        return new ExportTable("teachers", "Teachers", headers, rows);
    }

    private async Task<ExportTable> BuildSectionsAsync(CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var sections = await context.Sections
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        var gradeNames = await context.GradeLevels
            .AsNoTracking()
            .ToDictionaryAsync(g => g.Id, g => g.Name, ct);

        var yearNames = await context.SchoolYears
            .AsNoTracking()
            .ToDictionaryAsync(y => y.Id, y => y.Name, ct);

        // Teacher.FullName is computed in-memory, so materialize first.
        var adviserNames = (await context.Teachers
                .AsNoTracking()
                .Select(t => new { t.Id, t.FirstName, t.LastName, t.MiddleName })
                .ToListAsync(ct))
            .ToDictionary(t => t.Id, t => $"{t.LastName}, {t.FirstName}");

        var enrolledCounts = (await context.Enrollments
                .AsNoTracking()
                .Where(e => e.Status == "ACTIVE" || e.Status == "ENROLLED")
                .GroupBy(e => e.SectionId)
                .Select(g => new { SectionId = g.Key, Count = g.Count() })
                .ToListAsync(ct))
            .ToDictionary(x => x.SectionId, x => x.Count);

        string[] headers =
        [
            "Section", "Grade Level", "School Year", "Adviser", "Capacity", "Enrolled", "Archived"
        ];

        var rows = new List<string?[]>(sections.Count);
        foreach (var sec in sections)
        {
            rows.Add(
            [
                sec.Name,
                gradeNames.GetValueOrDefault(sec.GradeLevelId),
                yearNames.GetValueOrDefault(sec.SchoolYearId),
                sec.AdviserTeacherId.HasValue ? adviserNames.GetValueOrDefault(sec.AdviserTeacherId.Value) : null,
                sec.Capacity?.ToString(CultureInfo.InvariantCulture),
                enrolledCounts.GetValueOrDefault(sec.Id, 0).ToString(CultureInfo.InvariantCulture),
                sec.IsArchived ? "Yes" : "No"
            ]);
        }

        return new ExportTable("sections", "Sections", headers, rows, new HashSet<int> { 4, 5 });
    }

    private async Task<ExportTable> BuildTeacherSessionsAsync(ExportOptions options, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        // A detailed, denormalized view: one row per attendance record, carrying the
        // teacher who held the session, the session itself (date / section / subject /
        // status), and how the student was marked. This lets an admin see each teacher's
        // session history together with the students in every session. Inner joins mean
        // only sessions with an assigned teacher and a marked student appear.
        var query =
            from r in context.AttendanceRecords.AsNoTracking()
            join s in context.AttendanceSessions.AsNoTracking() on r.AttendanceSessionId equals s.Id
            join co in context.ClassOfferings.AsNoTracking() on s.ClassOfferingId equals co.Id
            join t in context.Teachers.AsNoTracking() on co.TeacherId equals (long?)t.Id
            join stu in context.Students.AsNoTracking() on r.StudentId equals stu.Id
            select new
            {
                TeacherId = t.Id,
                t.EmployeeNo,
                TeacherLast = t.LastName,
                TeacherFirst = t.FirstName,
                TeacherMiddle = t.MiddleName,
                s.SessionDate,
                s.SectionName,
                s.SubjectName,
                SessionStatus = s.Status,
                stu.Lrn,
                stu.StudentNumber,
                StudentLast = stu.LastName,
                StudentFirst = stu.FirstName,
                StudentMiddle = stu.MiddleName,
                Attendance = r.Status,
                MarkedAt = r.CreatedAt
            };

        if (options.TeacherId is { } teacherId)
            query = query.Where(x => x.TeacherId == teacherId);
        if (StartOf(options) is { } sessionsFrom)
            query = query.Where(x => x.SessionDate >= sessionsFrom);
        if (EndOf(options) is { } sessionsUntil)
            query = query.Where(x => x.SessionDate < sessionsUntil);
        if (StatusOf(options) is { } sessionsStatus)
            query = query.Where(x => x.Attendance == sessionsStatus);

        var records = await query
            .OrderBy(x => x.TeacherLast).ThenBy(x => x.TeacherFirst)
            .ThenBy(x => x.SessionDate)
            .ThenBy(x => x.StudentLast).ThenBy(x => x.StudentFirst)
            .ToListAsync(ct);

        // Name the scoped teacher for the title and file name. Fall back to a direct
        // lookup so a teacher with no marked records still gets a labelled (empty) export.
        string? scopedTeacher = null;
        if (options.TeacherId is { } scopedId)
        {
            var first = records.FirstOrDefault();
            if (first is not null)
            {
                scopedTeacher = FormatName(first.TeacherLast, first.TeacherFirst, first.TeacherMiddle);
            }
            else
            {
                var t = await context.Teachers
                    .AsNoTracking()
                    .Where(x => x.Id == scopedId)
                    .Select(x => new { x.LastName, x.FirstName, x.MiddleName })
                    .FirstOrDefaultAsync(ct);

                if (t is not null)
                    scopedTeacher = FormatName(t.LastName, t.FirstName, t.MiddleName);
            }
        }

        string[] headers =
        [
            "Employee No", "Teacher", "Session Date", "Section", "Subject", "Session Status",
            "Student LRN", "Student No", "Student", "Attendance", "Marked At"
        ];

        var rows = new List<string?[]>(records.Count);
        foreach (var x in records)
        {
            rows.Add(
            [
                x.EmployeeNo,
                FormatName(x.TeacherLast, x.TeacherFirst, x.TeacherMiddle),
                SessionDates.LocalDate(x.SessionDate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                x.SectionName,
                x.SubjectName,
                x.SessionStatus,
                x.Lrn,
                x.StudentNumber,
                FormatName(x.StudentLast, x.StudentFirst, x.StudentMiddle),
                x.Attendance,
                SessionDates.ToLocal(x.MarkedAt).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
            ]);
        }

        var (kind, title) = Label("teacher-sessions", "Teacher Sessions",
            scopedTeacher, DescribeRange(options), DescribeStatus(options));

        return new ExportTable(kind, title, headers, rows);
    }

    private async Task<ExportTable> BuildStudentAttendanceAsync(ExportOptions options, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        // One row per attendance record, ordered by student then date — the mirror image of
        // the teacher-sessions export. Left joins on the offering and teacher so a record
        // still exports when the class has no teacher assigned.
        var query =
            from r in context.AttendanceRecords.AsNoTracking()
            join s in context.AttendanceSessions.AsNoTracking() on r.AttendanceSessionId equals s.Id
            join stu in context.Students.AsNoTracking() on r.StudentId equals stu.Id
            join co in context.ClassOfferings.AsNoTracking() on s.ClassOfferingId equals co.Id into coJoin
            from co in coJoin.DefaultIfEmpty()
            join t in context.Teachers.AsNoTracking() on co.TeacherId equals (long?)t.Id into tJoin
            from t in tJoin.DefaultIfEmpty()
            select new
            {
                StudentId = stu.Id,
                SectionId = co != null ? (long?)co.SectionId : null,
                stu.Lrn,
                stu.StudentNumber,
                StudentLast = stu.LastName,
                StudentFirst = stu.FirstName,
                StudentMiddle = stu.MiddleName,
                s.SessionDate,
                s.SectionName,
                s.SubjectName,
                SessionStatus = s.Status,
                TeacherLast = t != null ? t.LastName : null,
                TeacherFirst = t != null ? t.FirstName : null,
                TeacherMiddle = t != null ? t.MiddleName : null,
                Attendance = r.Status,
                r.Reason,
                MarkedAt = r.CreatedAt
            };

        if (options.SectionId is { } sectionId)
            query = query.Where(x => x.SectionId == sectionId);
        if (options.StudentId is { } studentId)
            query = query.Where(x => x.StudentId == studentId);
        if (StartOf(options) is { } attendanceFrom)
            query = query.Where(x => x.SessionDate >= attendanceFrom);
        if (EndOf(options) is { } attendanceUntil)
            query = query.Where(x => x.SessionDate < attendanceUntil);
        if (StatusOf(options) is { } attendanceStatus)
            query = query.Where(x => x.Attendance == attendanceStatus);

        var records = await query
            .OrderBy(x => x.StudentLast).ThenBy(x => x.StudentFirst)
            .ThenBy(x => x.SessionDate)
            .ToListAsync(ct);

        // Grade level comes from the student's enrolment, not the session.
        var gradeByStudent = await (
            from e in context.Enrollments.AsNoTracking()
            join gl in context.GradeLevels.AsNoTracking() on e.GradeLevelId equals gl.Id
            select new { e.StudentId, gl.Name })
            .ToListAsync(ct);

        var gradeLookup = gradeByStudent
            .GroupBy(g => g.StudentId)
            .ToDictionary(g => g.Key, g => g.First().Name);

        string[] headers =
        [
            "Student LRN", "Student No", "Student", "Grade Level", "Section",
            "Session Date", "Subject", "Teacher", "Session Status", "Attendance", "Reason", "Marked At"
        ];

        var rows = new List<string?[]>(records.Count);
        foreach (var x in records)
        {
            rows.Add(
            [
                x.Lrn,
                x.StudentNumber,
                FormatName(x.StudentLast, x.StudentFirst, x.StudentMiddle),
                gradeLookup.GetValueOrDefault(x.StudentId),
                x.SectionName,
                SessionDates.LocalDate(x.SessionDate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                x.SubjectName,
                x.TeacherLast is null ? null : FormatName(x.TeacherLast, x.TeacherFirst!, x.TeacherMiddle),
                x.SessionStatus,
                x.Attendance,
                x.Reason,
                SessionDates.ToLocal(x.MarkedAt).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
            ]);
        }

        var scope = await DescribeStudentScopeAsync(context, options, records.Count > 0
            ? FormatName(records[0].StudentLast, records[0].StudentFirst, records[0].StudentMiddle)
            : null, ct);

        var (kind, title) = Label("student-attendance", "Student Attendance",
            scope, DescribeRange(options), DescribeStatus(options));

        return new ExportTable(kind, title, headers, rows);
    }

    private async Task<ExportTable> BuildAttendanceSummaryAsync(ExportOptions options, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        // One row per student: how many sessions they had in the period and how each was
        // marked. Left join on the offering so the section filter matches the
        // student-attendance export, and records without a teacher still count.
        var query =
            from r in context.AttendanceRecords.AsNoTracking()
            join s in context.AttendanceSessions.AsNoTracking() on r.AttendanceSessionId equals s.Id
            join stu in context.Students.AsNoTracking() on r.StudentId equals stu.Id
            join co in context.ClassOfferings.AsNoTracking() on s.ClassOfferingId equals co.Id into coJoin
            from co in coJoin.DefaultIfEmpty()
            select new
            {
                r.AttendanceSessionId,
                StudentId = stu.Id,
                SectionId = co != null ? (long?)co.SectionId : null,
                stu.Lrn,
                stu.StudentNumber,
                StudentLast = stu.LastName,
                StudentFirst = stu.FirstName,
                StudentMiddle = stu.MiddleName,
                s.SessionDate,
                s.SectionName,
                Attendance = r.Status,
                r.CreatedAt
            };

        if (options.SectionId is { } sectionId)
            query = query.Where(x => x.SectionId == sectionId);
        if (StartOf(options) is { } summaryFrom)
            query = query.Where(x => x.SessionDate >= summaryFrom);
        if (EndOf(options) is { } summaryUntil)
            query = query.Where(x => x.SessionDate < summaryUntil);

        var records = await query.ToListAsync(ct);
        var placements = await LoadPlacementsAsync(context, ct);

        string[] headers =
        [
            "Student LRN", "Student No", "Student", "Grade Level", "Section", "Sessions",
            "Present", "Late", "Absent", "Excused", "Attendance Rate (%)", "First Session", "Last Session"
        ];

        var rows = records
            // A student counts once per session: keep the latest mark if a session has several.
            .GroupBy(x => (x.AttendanceSessionId, x.StudentId))
            .Select(g => g.OrderByDescending(x => x.CreatedAt).First())
            .GroupBy(x => x.StudentId)
            .Select(g =>
            {
                var first = g.First();
                var present = g.Count(x => x.Attendance == "PRESENT");
                var late = g.Count(x => x.Attendance == "LATE");
                var absent = g.Count(x => x.Attendance == "ABSENT");
                var excused = g.Count(x => x.Attendance == "EXCUSED");

                // Same rule as the at-risk report: excused absences don't count against the student.
                var counted = present + late + absent;
                double? rate = counted > 0 ? Math.Round((double)(present + late) / counted * 100, 1) : null;

                placements.TryGetValue(g.Key, out var placement);
                var latestSection = g.OrderByDescending(x => x.SessionDate).First().SectionName;

                return new
                {
                    first.StudentLast,
                    first.StudentFirst,
                    Row = new string?[]
                    {
                        first.Lrn,
                        first.StudentNumber,
                        FormatName(first.StudentLast, first.StudentFirst, first.StudentMiddle),
                        placement?.GradeName,
                        placement?.SectionName ?? latestSection,
                        g.Count().ToString(CultureInfo.InvariantCulture),
                        present.ToString(CultureInfo.InvariantCulture),
                        late.ToString(CultureInfo.InvariantCulture),
                        absent.ToString(CultureInfo.InvariantCulture),
                        excused.ToString(CultureInfo.InvariantCulture),
                        rate?.ToString("0.0", CultureInfo.InvariantCulture),
                        SessionDates.LocalDate(g.Min(x => x.SessionDate)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        SessionDates.LocalDate(g.Max(x => x.SessionDate)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    }
                };
            })
            .OrderBy(x => x.StudentLast, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(x => x.StudentFirst, StringComparer.CurrentCultureIgnoreCase)
            .Select(x => x.Row)
            .ToList();

        string? sectionName = null;
        if (options.SectionId is { } scopedSection)
        {
            sectionName = await context.Sections
                .AsNoTracking()
                .Where(x => x.Id == scopedSection)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(ct);
        }

        var (kind, title) = Label("attendance-summary", "Attendance Summary", sectionName, DescribeRange(options));

        return new ExportTable(kind, title, headers, rows, new HashSet<int> { 5, 6, 7, 8, 9, 10 });
    }

    // ── Filter helpers ──

    // Session dates are stored as UTC, so the local From/To days are converted to UTC bounds.

    /// <summary>Inclusive lower bound on session dates (UTC), or null when unbounded.</summary>
    private static DateTime? StartOf(ExportOptions options) =>
        options.From is { } from ? SessionDates.LocalDayStartUtc(from) : null;

    /// <summary>Exclusive upper bound (start of the local day after <see cref="ExportOptions.To"/>, in UTC), so sessions at any time that day count.</summary>
    private static DateTime? EndOf(ExportOptions options) =>
        options.To is { } to ? SessionDates.LocalDayStartUtc(to.AddDays(1)) : null;

    /// <summary>The normalised status filter, or null when absent or not a known status.</summary>
    private static string? StatusOf(ExportOptions options)
    {
        var status = options.Status?.Trim().ToUpperInvariant();
        return status is not null && KnownStatuses.Contains(status) ? status : null;
    }

    /// <summary>Human-readable date range, e.g. "2026-10-01 to 2026-10-31".</summary>
    private static string? DescribeRange(ExportOptions options) =>
        (options.From, options.To) switch
        {
            (null, null) => null,
            ({ } from, null) => $"from {from:yyyy-MM-dd}",
            (null, { } to) => $"until {to:yyyy-MM-dd}",
            ({ } from, { } to) when from == to => $"{from:yyyy-MM-dd}",
            ({ } from, { } to) => $"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}"
        };

    private static string? DescribeStatus(ExportOptions options) =>
        StatusOf(options) is { } status
            ? $"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(status.ToLowerInvariant())} only"
            : null;

    /// <summary>
    /// Builds the file kind and display title from a base name plus whatever the export was
    /// narrowed to, e.g. ("student-attendance-grade-7-a-absent-only", "Student Attendance — Grade 7-A · Absent only").
    /// </summary>
    private static (string Kind, string Title) Label(string baseKind, string baseTitle, params string?[] scopes)
    {
        var parts = scopes.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!).ToList();
        if (parts.Count == 0)
            return (baseKind, baseTitle);

        return ($"{baseKind}-{string.Join("-", parts.Select(Slug))}", $"{baseTitle} — {string.Join(" · ", parts)}");
    }

    /// <summary>Names whatever the student-attendance export was narrowed to, for the title and file name.</summary>
    private static async Task<string?> DescribeStudentScopeAsync(
        SchoolDbContext context, ExportOptions options, string? firstRowStudent, CancellationToken ct)
    {
        if (options.StudentId is { } studentId)
        {
            if (firstRowStudent is not null)
                return firstRowStudent;

            var s = await context.Students
                .AsNoTracking()
                .Where(x => x.Id == studentId)
                .Select(x => new { x.LastName, x.FirstName, x.MiddleName })
                .FirstOrDefaultAsync(ct);

            return s is null ? null : FormatName(s.LastName, s.FirstName, s.MiddleName);
        }

        if (options.SectionId is { } sectionId)
        {
            return await context.Sections
                .AsNoTracking()
                .Where(x => x.Id == sectionId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(ct);
        }

        return null;
    }

    private static string FormatName(string last, string first, string? middle) =>
        $"{last}, {first}{(string.IsNullOrWhiteSpace(middle) ? "" : $" {middle}")}";

    /// <summary>Turns a display name into a file-name-safe slug (e.g. "Santos, Maria" → "santos-maria").</summary>
    private static string Slug(string value)
    {
        var sb = new StringBuilder(value.Length);
        var lastWasDash = true;   // leading dashes are dropped

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
                lastWasDash = false;
            }
            else if (!lastWasDash)
            {
                sb.Append('-');
                lastWasDash = true;
            }
        }

        return sb.ToString().Trim('-') is { Length: > 0 } slug ? slug : "export";
    }

    // ── CSV helpers ──

    private static ExportFile ToFile(ExportTable table)
    {
        var sb = new StringBuilder();
        WriteRow(sb, [.. table.Headers]);
        foreach (var row in table.Rows)
            WriteRow(sb, row);

        return new ExportFile(BuildFileName(table.Kind), Encode(sb));
    }

    private static ExportFile ToWorkbook(string kind, IReadOnlyList<ExportTable> tables)
    {
        // Tab names are capped at 31 characters, so they carry only the dataset name
        // ("Teacher Sessions"); the filters are already in the file name.
        var sheets = tables
            .Select(t => new XlsxSheet(t.Title.Split(" — ")[0], t.Headers, t.Rows, t.NumericColumns))
            .ToList();

        return new ExportFile(BuildFileName(kind, "xlsx"), XlsxWriter.Write(sheets));
    }

    private static string BuildFileName(string kind, string extension = "csv") =>
        $"{kind}-{DateTime.Now:yyyyMMdd-HHmmss}.{extension}";

    private static byte[] Encode(StringBuilder sb) =>
        Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
            .ToArray();

    private static void WriteRow(StringBuilder sb, params string?[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(Escape(values[i]));
        }
        sb.Append("\r\n");
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Stop spreadsheet apps from evaluating a cell as a formula (CSV injection).
        if (value[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
            value = "'" + value;

        var needsQuoting = value.Contains(',') || value.Contains('"')
            || value.Contains('\n') || value.Contains('\r');

        if (!needsQuoting)
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
