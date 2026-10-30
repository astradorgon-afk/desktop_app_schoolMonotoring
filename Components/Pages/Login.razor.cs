using AttendanceUI.Data;
using AttendanceUI.Data.Entities;
using AttendanceUI.Models.Auth;
using AttendanceUI.Services;
using Microsoft.AspNetCore.Components;

namespace AttendanceUI.Components.Pages;

public partial class Login : ComponentBase
{
    private LoginRequest loginModel = new() { Username = "admin", Password = "admin" };
    private bool isLoading;
    private string? errorMessage;
    private string? usernameError;
    private string? passwordError;
    private bool _showPassword;
    private string selectedDbProfile = "";

    private IReadOnlyList<AttendanceUI.Data.ConnectionProfile> DbProfiles => DatabaseSelector.Profiles;

    private string? SelectedProfileDescription =>
        DbProfiles.FirstOrDefault(p => p.Key == selectedDbProfile)?.Description;

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private DatabaseSelector DatabaseSelector { get; set; } = null!;

    [Inject]
    private AttendanceUI.Data.SwitchableDbContextFactory DbContextFactory { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    protected override void OnInitialized()
    {
        selectedDbProfile = DatabaseSelector.Current.Key;
    }

    private async Task HandleLogin()
    {
        usernameError = null;
        passwordError = null;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(loginModel.Username))
        {
            usernameError = "Username is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(loginModel.Password))
        {
            passwordError = "Password is required.";
            return;
        }

        isLoading = true;

        // Paint the spinner before any database work starts. Preparing a SQLite
        // profile for the first time creates the schema and seeds the demo dataset,
        // which is slow enough to look like a hang without this.
        StateHasChanged();
        await Task.Yield();

        try
        {
            // Point the app at the chosen database before authenticating.
            DatabaseSelector.Select(selectedDbProfile);

            // Runs off the UI thread. Without this the first-run seed blocks the
            // renderer long enough for Android to raise "app isn't responding".
            await DbContextFactory.EnsureInitializedAsync();

            var result = await AuthService.LoginAsync(loginModel);

            if (result.Success)
            {
                var target = result.Session?.Role switch
            {
                UserRole.TEACHER => "/teacher/dashboard",
                UserRole.STUDENT => "/student/dashboard",
                _ => "/dashboard"
            };
                Navigation.NavigateTo(target);
            }
            else
            {
                errorMessage = result.Message ?? "Invalid username or password.";
            }
        }
        catch (Exception ex)
        {
            var profileName = DbProfiles.FirstOrDefault(p => p.Key == selectedDbProfile)?.DisplayName ?? selectedDbProfile;
            errorMessage = DescribeConnectionFailure(profileName, ex);
        }
        finally
        {
            isLoading = false;
        }
    }

    // MySQL reports schema drift as ordinary query errors (1054 unknown column,
    // 1146 missing table). Those mean the app reached the server fine and it is the
    // schema that is behind, so point at the catch-up migration rather than leaving
    // the raw driver text as a bare "could not connect".
    private static string DescribeConnectionFailure(string profileName, Exception ex)
    {
        var message = (ex.InnerException ?? ex).Message;

        var isSchemaDrift =
            message.Contains("Unknown column", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("Unknown table", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase);

        if (isSchemaDrift)
        {
            return $"Connected to the \"{profileName}\" database, but its schema is out of date " +
                   $"({message}). Run Migrations/Hostinger_CatchUp.sql once against that database " +
                   "— hPanel → Databases → phpMyAdmin → SQL tab — to add the missing columns and tables.";
        }

        return $"Could not connect to the \"{profileName}\" database. {message}";
    }
}
