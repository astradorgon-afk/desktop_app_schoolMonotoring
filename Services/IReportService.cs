namespace AttendanceUI.Services;

public class ReportSummary
{
    public int TotalSessions { get; init; }
    public int TotalRecords { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public double PresentPercent => TotalRecords > 0 ? Math.Round((double)PresentCount / TotalRecords * 100, 1) : 0;
    public double LatePercent => TotalRecords > 0 ? Math.Round((double)LateCount / TotalRecords * 100, 1) : 0;
    public double AbsentPercent => TotalRecords > 0 ? Math.Round((double)AbsentCount / TotalRecords * 100, 1) : 0;
    public double AttendanceRate => TotalRecords > 0 ? Math.Round((double)(PresentCount + LateCount) / TotalRecords * 100, 1) : 0;
}

public class SectionReportItem
{
    public string SectionName { get; init; } = string.Empty;
    public int EnrolledStudents { get; init; }
    public int TotalRecords { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public double PresentPercent => TotalRecords > 0 ? Math.Round((double)PresentCount / TotalRecords * 100, 1) : 0;
    public double LatePercent => TotalRecords > 0 ? Math.Round((double)LateCount / TotalRecords * 100, 1) : 0;
    public double AbsentPercent => TotalRecords > 0 ? Math.Round((double)AbsentCount / TotalRecords * 100, 1) : 0;
    public double AttendanceRate => TotalRecords > 0 ? Math.Round((double)(PresentCount + LateCount) / TotalRecords * 100, 1) : 0;
}

public class DailyTrendItem
{
    public DateOnly Date { get; init; }
    public int Sessions { get; init; }
    public int PresentCount { get; init; }
    public int LateCount { get; init; }
    public int AbsentCount { get; init; }
    public int TotalRecords => PresentCount + LateCount + AbsentCount;
    public double PresentPercent => TotalRecords > 0 ? Math.Round((double)PresentCount / TotalRecords * 100, 1) : 0;
}

public class ReportData
{
    public ReportSummary Summary { get; init; } = new();
    public IReadOnlyList<SectionReportItem> SectionReports { get; init; } = [];
    public IReadOnlyList<DailyTrendItem> DailyTrend { get; init; } = [];
    public DateOnly MinDate { get; init; }
    public DateOnly MaxDate { get; init; }
}

public interface IReportService
{
    Task<ReportData> GetReportAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);
}
