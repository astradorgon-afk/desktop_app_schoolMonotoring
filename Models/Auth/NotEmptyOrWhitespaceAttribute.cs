using System.ComponentModel.DataAnnotations;

namespace AttendanceUI.Models.Auth;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NotEmptyOrWhitespaceAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is string str && !string.IsNullOrWhiteSpace(str);
    }
}
