using System.Resources;
using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using AttendanceUI.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AttendanceUI.Services;

public class UserService : IUserService
{
    private readonly IDbContextFactory<SchoolDbContext> _contextFactory;
    private readonly ILogger<UserService> _logger;
    private readonly ResourceManager _logMessages = new(typeof(LogMessages));

    public UserService(IDbContextFactory<SchoolDbContext> contextFactory, ILogger<UserService> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Users.FindAsync([id], cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(username);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Users
            .Where(u => u.Username == username)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Users
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CreateAsync(User user, string plainPassword, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(plainPassword);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var exists = await context.Users
            .AnyAsync(u => u.Username == user.Username, cancellationToken);

        if (exists)
            return false;

        user.PasswordHash = PasswordHelper.Hash(plainPassword);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            _logMessages.GetString("User_Created") ?? "User '{Username}' created.",
            user.Username);

        return true;
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await context.Users.FindAsync([user.Id], cancellationToken);
        if (existing is null)
            return false;

        existing.Username = user.Username;
        existing.Role = user.Role;
        existing.Status = user.Status;
        existing.CanLogin = user.CanLogin;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            _logMessages.GetString("User_Updated") ?? "User '{Username}' updated.",
            user.Username);

        return true;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var user = await context.Users.FindAsync([id], cancellationToken);
        if (user is null)
            return false;

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            _logMessages.GetString("User_Deleted") ?? "User '{Username}' deleted.",
            user.Username);

        return true;
    }

    public async Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user is null)
            return false;

        if (!PasswordHelper.Verify(currentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = PasswordHelper.Hash(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            _logMessages.GetString("Auth_PasswordChanged") ?? "Password changed for user '{Username}'.",
            user.Username);

        return true;
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string plainPassword, CancellationToken cancellationToken = default)
    {
        var user = await GetByUsernameAsync(username, cancellationToken);
        if (user is null)
            return false;

        return PasswordHelper.Verify(plainPassword, user.PasswordHash);
    }
}
