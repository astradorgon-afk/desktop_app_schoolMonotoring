using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace AttendanceUI.Services;

public class RoomService : IRoomService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private readonly ILogger<RoomService> _logger;

    public RoomService(IDbContextFactory<SchoolDbContext> contextFactory, ILogger<RoomService> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<Room>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        List<Room> rooms;
        try
        {
            rooms = await context.Rooms
                .Include(r => r.QrCode)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            rooms = await context.Rooms
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        var scheduledRoomIds = await context.ClassSchedules
            .Where(s => s.RoomId != null)
            .Select(s => s.RoomId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var scheduledSet = scheduledRoomIds.ToHashSet();
        foreach (var room in rooms)
            room.HasSchedule = scheduledSet.Contains(room.Id);

        return rooms;
    }

    public async Task<List<RoomQr>> GenerateMissingBatchAsync(List<Room> rooms, CancellationToken cancellationToken = default)
    {
        if (rooms.Count == 0)
            return [];

        var needsQr = rooms.Where(r => r.QrCode is null).ToList();
        if (needsQr.Count == 0)
            return [];

        var now = DateTime.UtcNow;
        var newQrs = new List<RoomQr>(needsQr.Count);

        using var generator = new QRCodeGenerator();

        foreach (var room in needsQr)
        {
            var qrContent = $"ATTENDANCE:ROOM:{room.Code}:{room.Name}";
            using var qrData = generator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            using var png = new PngByteQRCode(qrData);

            var roomQr = new RoomQr
            {
                RoomId = room.Id,
                QrContent = qrContent,
                QrCode = png.GetGraphic(10),
                CreatedAt = now,
                UpdatedAt = now
            };
            newQrs.Add(roomQr);
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.AddRange(newQrs);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var room in needsQr)
        {
            room.QrCode = newQrs.First(q => q.RoomId == room.Id);
        }

        _logger.LogInformation("Generated {Count} QR codes for rooms.", newQrs.Count);
        return newQrs;
    }
}
