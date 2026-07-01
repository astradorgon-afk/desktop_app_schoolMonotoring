using AttendanceUI.Models.Auth;
using AttendanceUI.Services;
using Microsoft.AspNetCore.Components;

namespace AttendanceUI.Components.Pages;

public partial class Login : ComponentBase
{
    private LoginRequest loginModel = new();
    private bool isLoading;
    private string? errorMessage;
    private string? usernameError;
    private string? passwordError;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

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

        try
        {
            var result = await AuthService.LoginAsync(loginModel);

            if (result.Success)
            {
                Navigation.NavigateTo("/dashboard");
            }
            else
            {
                errorMessage = result.Message ?? "Invalid username or password.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }
}
