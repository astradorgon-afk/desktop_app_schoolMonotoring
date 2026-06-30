using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace AttendanceUI.Components.Pages;

public partial class Login : ComponentBase
{
    private LoginModel loginModel = new();
    private bool isLoading;
    private string? errorMessage;
    private string? emailError;
    private string? passwordError;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private async Task HandleLogin()
    {
        emailError = null;
        passwordError = null;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(loginModel.Email))
        {
            emailError = "Email is required.";
            return;
        }

        if (!Regex.IsMatch(loginModel.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            emailError = "Enter a valid email address.";
            return;
        }

        if (string.IsNullOrWhiteSpace(loginModel.Password))
        {
            passwordError = "Password is required.";
            return;
        }

        if (loginModel.Password.Length < 6)
        {
            passwordError = "Password must be at least 6 characters.";
            return;
        }

        isLoading = true;

        try
        {
            await Task.Delay(1500);

            if (loginModel.Email == "admin@example.com" && loginModel.Password == "admin123")
            {
                Navigation.NavigateTo("/dashboard");
            }
            else
            {
                errorMessage = "Invalid email or password. Please try again.";
            }
        }
        finally
        {
            isLoading = false;
        }
    }

    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
