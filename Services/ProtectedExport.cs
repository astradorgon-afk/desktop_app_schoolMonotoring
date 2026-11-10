namespace AttendanceUI.Services;

/// <summary>
/// Password-protects exported workbooks. Every export is an Excel file (.xlsx) encrypted the
/// way Excel's own "Encrypt with Password" does it (AES-256, see <see cref="OfficeEncryption"/>),
/// so double-clicking it opens Excel, which asks for the password; no other tool is needed.
/// The password is never stored; a forgotten password means the file cannot be opened.
/// </summary>
public static class ProtectedExport
{
    public const int MinPasswordLength = 8;

    /// <summary>Shown to the user next to the saved path.</summary>
    public const string OpenHint = "Open it in Excel and enter the password.";

    /// <summary>Returns why the password cannot be used, or null when it is acceptable.</summary>
    public static string? Validate(string? password, string? confirmation)
    {
        if (string.IsNullOrEmpty(password))
            return "Enter a password to protect the export.";
        if (password.Length < MinPasswordLength)
            return $"The password must be at least {MinPasswordLength} characters.";
        if (password != confirmation)
            return "The passwords do not match.";
        return null;
    }

    /// <summary>Encrypts an .xlsx export with <paramref name="password"/>, keeping its file name.</summary>
    public static ExportFile Protect(ExportFile workbook, string password)
    {
        ArgumentNullException.ThrowIfNull(workbook);
        ArgumentException.ThrowIfNullOrEmpty(password);
        if (!workbook.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only Excel workbooks can be password-protected.", nameof(workbook));

        return workbook with { Content = OfficeEncryption.Encrypt(workbook.Content, password) };
    }
}
