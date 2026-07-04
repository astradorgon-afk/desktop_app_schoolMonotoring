using System.Resources;
using System.Text.Json;
using AttendanceUI.Data.Entities;
using AttendanceUI.Models.Auth;
using AttendanceUI.Resources;
using Microsoft.Extensions.Logging;

namespace AttendanceUI.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthService> _logger;
    private readonly ResourceManager _errorMessages = new(typeof(ErrorMessages));
    private readonly ResourceManager _logMessages = new(typeof(LogMessages));

    private UserSession? _currentSession;

    private const string SessionPrefKey = "auth_session";

    public AuthService(IUserService userService, ILogger<AuthService> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsAuthenticated => _currentSession is not null;

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            _logMessages.GetString("Auth_LoginAttempt") ?? "Login attempt for user '{Username}'.",
            request.Username);

        var user = await _userService.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning(
                _logMessages.GetString("Auth_LoginFailed") ?? "Login failed for user '{Username}'. Reason: {Reason}.",
                request.Username, "User not found");

            return LoginResponse.Fail(
                _errorMessages.GetString("Auth_InvalidCredentials") ?? "Invalid username or password.");
        }

        if (user.Status == UserStatus.LOCKED)
        {
            _logger.LogWarning(
                _logMessages.GetString("Auth_LoginFailed") ?? "Login failed for user '{Username}'. Reason: {Reason}.",
                request.Username, "Account locked");

            return LoginResponse.Fail(
                _errorMessages.GetString("Auth_AccountLocked") ?? "Account is locked.");
        }

        if (user.Status == UserStatus.INACTIVE)
        {
            _logger.LogWarning(
                _logMessages.GetString("Auth_LoginFailed") ?? "Login failed for user '{Username}'. Reason: {Reason}.",
                request.Username, "Account inactive");

            return LoginResponse.Fail(
                _errorMessages.GetString("Auth_AccountInactive") ?? "Account is inactive.");
        }

        if (!user.CanLogin)
        {
            _logger.LogWarning(
                _logMessages.GetString("Auth_LoginFailed") ?? "Login failed for user '{Username}'. Reason: {Reason}.",
                request.Username, "Login disabled");

            return LoginResponse.Fail(
                _errorMessages.GetString("Auth_LoginDisabled") ?? "Login is disabled.");
        }

        var isValid = await _userService.ValidateCredentialsAsync(request.Username, request.Password, cancellationToken);
        if (!isValid)
        {
            _logger.LogWarning(
                _logMessages.GetString("Auth_LoginFailed") ?? "Login failed for user '{Username}'. Reason: {Reason}.",
                request.Username, "Invalid credentials");

            return LoginResponse.Fail(
                _errorMessages.GetString("Auth_InvalidCredentials") ?? "Invalid username or password.");
        }

        _currentSession = new UserSession
        {
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            DisplayName = !string.IsNullOrWhiteSpace(user.Name) ? user.Name : user.Username,
            LoggedInAt = DateTime.UtcNow
        };

        if (request.RememberMe)
        {
            PersistSession(_currentSession);
        }
        else
        {
            ClearPersistedSession();
        }

        _logger.LogInformation(
            _logMessages.GetString("Auth_LoginSuccess") ?? "User '{Username}' logged in successfully.",
            request.Username);

        return LoginResponse.Ok(_currentSession);
    }

    public Task LogoutAsync()
    {
        if (_currentSession is not null)
        {
            _logger.LogInformation(
                _logMessages.GetString("Auth_Logout") ?? "User '{Username}' logged out.",
                _currentSession.Username);
        }

        _currentSession = null;
        ClearPersistedSession();
        return Task.CompletedTask;
    }

    public Task<UserSession?> GetCurrentSessionAsync()
    {
        if (_currentSession is not null)
            return Task.FromResult(_currentSession);

        _currentSession = LoadPersistedSession();
        return Task.FromResult(_currentSession);
    }

    private static void PersistSession(UserSession session)
    {
        try
        {
            var json = JsonSerializer.Serialize(session);
            Preferences.Set(SessionPrefKey, json);
        }
        catch
        {
            // Silently fail - persistence is best-effort
        }
    }

    private static UserSession? LoadPersistedSession()
    {
        try
        {
            var json = Preferences.Get(SessionPrefKey, null);
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonSerializer.Deserialize<UserSession>(json);
        }
        catch
        {
            ClearPersistedSession();
            return null;
        }
    }

    private static void ClearPersistedSession()
    {
        try
        {
            if (Preferences.ContainsKey(SessionPrefKey))
                Preferences.Remove(SessionPrefKey);
        }
        catch
        {
            // Silently fail
        }
    }
}
