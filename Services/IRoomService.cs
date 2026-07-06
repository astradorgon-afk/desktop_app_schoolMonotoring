using AttendanceUI.Data.Entities;

namespace AttendanceUI.Services;

public interface IRoomService
{
    Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<RoomQr>> GenerateMissingBatchAsync(List<Room> rooms, CancellationToken cancellationToken = default);
}
