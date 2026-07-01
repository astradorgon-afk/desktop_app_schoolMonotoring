using System.Resources;

namespace AttendanceUI.Services;

public static class ResourceHelper
{
    private static readonly ResourceManager ErrorManager = new("AttendanceUI.Resources.ErrorMessages", typeof(ResourceHelper).Assembly);
    private static readonly ResourceManager LogManager = new("AttendanceUI.Resources.LogMessages", typeof(ResourceHelper).Assembly);

    public static string GetError(string key) =>
        ErrorManager.GetString(key) ?? key;

    public static string GetLog(string key) =>
        LogManager.GetString(key) ?? key;
}
