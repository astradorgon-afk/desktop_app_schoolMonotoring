namespace AttendanceUI.Services;

/// <summary>Which set of records to export.</summary>
public enum ExportDataset
{
    Students,
    Teachers,
    Sections,
    TeacherSessions,
    StudentAttendance
}

/// <summary>A single generated export file: its suggested name and CSV bytes.</summary>
public sealed record ExportFile(string FileName, byte[] Content);

/// <summary>
/// Optional narrowing applied when building an export. <see cref="TeacherId"/> scopes the
/// teacher-sessions dataset; <see cref="SectionId"/> and <see cref="StudentId"/> scope the
/// student-attendance dataset. Each is ignored by the datasets it does not apply to.
/// </summary>
public sealed record ExportOptions(long? TeacherId = null, long? SectionId = null, long? StudentId = null)
{
    public static readonly ExportOptions None = new();
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
    /// <summary>Builds a CSV export for a single dataset.</summary>
    Task<ExportFile> ExportAsync(ExportDataset dataset, ExportOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Builds CSV exports for every dataset (students, teachers, sections, teacher sessions).</summary>
    Task<IReadOnlyList<ExportFile>> ExportAllAsync(ExportOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Builds a tabular preview (headers + rows) of a single dataset.</summary>
    Task<ExportPreview> PreviewAsync(ExportDataset dataset, ExportOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Builds a preview of every dataset, in the same order as <see cref="ExportAllAsync"/>.</summary>
    Task<IReadOnlyList<ExportPreview>> PreviewAllAsync(ExportOptions? options = null,
        CancellationToken cancellationToken = default);

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
