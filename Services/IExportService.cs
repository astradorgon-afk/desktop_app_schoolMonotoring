namespace AttendanceUI.Services;

/// <summary>Which set of records to export.</summary>
public enum ExportDataset
{
    Students,
    Teachers,
    Sections,
    TeacherSessions,
    StudentAttendance,
    AttendanceSummary
}

/// <summary>File format an export is written in.</summary>
public enum ExportFormat
{
    Csv,
    Excel
}

/// <summary>A single generated export file: its suggested name and contents (CSV or .xlsx bytes).</summary>
public sealed record ExportFile(string FileName, byte[] Content);

/// <summary>
/// Optional narrowing applied when building an export. Each filter is ignored by the
/// datasets it does not apply to:
/// <list type="bullet">
/// <item><see cref="TeacherId"/>: teacher sessions.</item>
/// <item><see cref="SectionId"/>: students (current section), teacher sessions, student attendance
/// and the attendance summary.</item>
/// <item><see cref="StudentId"/>: student attendance.</item>
/// <item><see cref="From"/>/<see cref="To"/> (inclusive session dates): every attendance dataset.</item>
/// <item><see cref="Status"/> (PRESENT, LATE, ABSENT or EXCUSED): the two per-record attendance datasets.</item>
/// <item><see cref="GradeLevelId"/>: students, sections and the attendance summary.</item>
/// <item><see cref="SchoolYearId"/> and <see cref="Archived"/>: sections.</item>
/// <item><see cref="Sex"/>: students. <see cref="RecordStatus"/>: students and teachers.</item>
/// <item><see cref="EmploymentStatus"/>: teachers.</item>
/// <item><see cref="Subject"/>: teacher sessions and student attendance.</item>
/// <item><see cref="Search"/> (name, LRN, student / employee number): students, teachers and the attendance summary.</item>
/// <item><see cref="RateBelow"/>: the attendance summary (students whose rate is under this percentage).</item>
/// </list>
/// </summary>
public sealed record ExportOptions(
    long? TeacherId = null,
    long? SectionId = null,
    long? StudentId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    string? Status = null)
{
    public long? GradeLevelId { get; init; }
    public long? SchoolYearId { get; init; }
    public bool? Archived { get; init; }
    public string? Sex { get; init; }
    public string? RecordStatus { get; init; }
    public string? EmploymentStatus { get; init; }
    public string? Subject { get; init; }
    public string? Search { get; init; }
    public double? RateBelow { get; init; }

    public static readonly ExportOptions None = new();
}

/// <summary>An id + name pair offered in an export filter dropdown (grade levels, school years).</summary>
public sealed record NamedOption(long Id, string Name);

/// <summary>The values offered by the export filter dropdowns, read from the data itself.</summary>
public sealed record ExportFilterChoices(
    IReadOnlyList<NamedOption> GradeLevels,
    IReadOnlyList<NamedOption> SchoolYears,
    IReadOnlyList<string> Sexes,
    IReadOnlyList<string> StudentStatuses,
    IReadOnlyList<string> TeacherStatuses,
    IReadOnlyList<string> EmploymentStatuses,
    IReadOnlyList<string> Subjects)
{
    public static readonly ExportFilterChoices Empty = new([], [], [], [], [], [], []);
}

/// <summary>A teacher offered in the export filter dropdown.</summary>
public sealed record TeacherOption(long Id, string Name, string? EmployeeNo);

/// <summary>A section offered in the export filter dropdown.</summary>
public sealed record SectionOption(long Id, string Name, string? GradeLevel);

/// <summary>A student offered in the export filter dropdown.</summary>
public sealed record StudentOption(long Id, string Name, string? Lrn);

/// <summary>
/// A tabular preview of an export: the column headers and the (possibly truncated)
/// rows, plus the total row count so the UI can note when rows were trimmed.
/// </summary>
public sealed record ExportPreview(
    string Title,
    IReadOnlyList<string> Headers,
    IReadOnlyList<string?[]> Rows,
    int TotalRows,
    ExportDataset Dataset);

public interface IExportService
{
    /// <summary>Builds a CSV or Excel export for a single dataset.</summary>
    Task<ExportFile> ExportAsync(ExportDataset dataset, ExportOptions? options = null,
        CancellationToken cancellationToken = default, ExportFormat format = ExportFormat.Csv);

    /// <summary>
    /// Builds an export of every dataset: one CSV file per dataset, or a single Excel
    /// workbook with one sheet per dataset. <paramref name="options"/> gives each dataset
    /// its own filters; a dataset missing from it is exported unfiltered.
    /// </summary>
    Task<IReadOnlyList<ExportFile>> ExportAllAsync(IReadOnlyDictionary<ExportDataset, ExportOptions>? options = null,
        CancellationToken cancellationToken = default, ExportFormat format = ExportFormat.Csv);

    /// <summary>Builds a tabular preview (headers + rows) of a single dataset.</summary>
    Task<ExportPreview> PreviewAsync(ExportDataset dataset, ExportOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Builds a preview of every dataset, in the same order as <see cref="ExportAllAsync"/>.</summary>
    Task<IReadOnlyList<ExportPreview>> PreviewAllAsync(IReadOnlyDictionary<ExportDataset, ExportOptions>? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Every dataset, in the order "Export All" produces them.</summary>
    IReadOnlyList<ExportDataset> AllDatasets { get; }

    /// <summary>Values for the grade, school year, sex, status, employment and subject filters.</summary>
    Task<ExportFilterChoices> GetFilterChoicesAsync(CancellationToken cancellationToken = default);

    /// <summary>Teachers that actually hold sessions, for the teacher-sessions export filter.</summary>
    Task<IReadOnlyList<TeacherOption>> GetTeacherOptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Sections available as a student-attendance export filter.</summary>
    Task<IReadOnlyList<SectionOption>> GetSectionOptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Students available as a student-attendance export filter, optionally limited to
    /// those enrolled in <paramref name="sectionId"/>.
    /// </summary>
    Task<IReadOnlyList<StudentOption>> GetStudentOptionsAsync(long? sectionId = null,
        CancellationToken cancellationToken = default);
}
