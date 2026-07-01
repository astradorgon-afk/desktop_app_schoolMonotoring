using AttendanceUI.Data.Entities;

namespace AttendanceUI.Services;

public class ScheduleViewEntry
{
    public long Id { get; init; }
    public long OfferingId { get; init; }
    public byte DayOfWeek { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string GradeLevel { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string TeacherName { get; init; } = string.Empty;
    public string? Room { get; init; }
    public string OfferingStatus { get; init; } = string.Empty;
    public long SectionId { get; init; }
    public long SubjectId { get; init; }
    public long? TeacherId { get; init; }
}

public class SectionCardInfo
{
    public long SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string GradeLevel { get; init; } = string.Empty;
    public long GradeLevelId { get; init; }
    public int StudentCount { get; init; }
}

public class SchedulePageData
{
    public List<SectionCardInfo> Sections { get; init; } = [];
    public List<ScheduleViewEntry> Entries { get; init; } = [];
}

public interface IScheduleService
{
    Task<SchedulePageData> GetPageDataAsync(string? gradeLevel = null, string? status = null, CancellationToken ct = default);
    Task<List<Subject>> GetSubjectsAsync(CancellationToken ct = default);
    Task<List<Teacher>> GetTeachersAsync(CancellationToken ct = default);
    Task<List<Section>> GetSectionsAsync(CancellationToken ct = default);
    Task<List<Room>> GetRoomsAsync(CancellationToken ct = default);
    Task AddScheduleEntryAsync(long sectionId, long subjectId, long teacherId, byte dayOfWeek,
        TimeOnly startTime, TimeOnly endTime, string? room, CancellationToken ct = default);
    Task UpdateScheduleEntryAsync(long offeringId, long scheduleId, long subjectId, long teacherId,
        byte dayOfWeek, TimeOnly startTime, TimeOnly endTime, string? room, CancellationToken ct = default);
    Task DeleteScheduleEntryAsync(long scheduleId, long offeringId, CancellationToken ct = default);
}
