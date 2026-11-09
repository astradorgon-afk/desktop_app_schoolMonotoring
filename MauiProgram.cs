using System.Text.Json;
using AttendanceUI.Data;
using AttendanceUI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AttendanceUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Sensitive columns are encrypted with this key (see Data/DataEncryption.cs).
		DataEncryption.Configure(ReadConfiguredDataKey());

		// Database connection is switchable at runtime (chosen on the login page).
		// The initial profile comes from the SCHOOL_SMS_DB_PROFILE environment
		// variable, defaulting to "local". SQLite profiles are created + seeded
		// lazily on first use by the factory.
		builder.Services.AddSingleton<DatabaseSelector>();
		// Registered concretely as well so the login page can await EnsureInitializedAsync
		// before the first query. Both registrations resolve to the same instance.
		builder.Services.AddSingleton<SwitchableDbContextFactory>();
		builder.Services.AddSingleton<IDbContextFactory<SchoolDbContext>>(
			sp => sp.GetRequiredService<SwitchableDbContextFactory>());

		builder.Services.AddSingleton<IUserService, UserService>();
		builder.Services.AddSingleton<IAuthService, AuthService>();
		builder.Services.AddSingleton<IAttendanceSessionService, AttendanceSessionService>();
		builder.Services.AddSingleton<IStudentService, StudentService>();
		builder.Services.AddSingleton<ITeacherService, TeacherService>();
		builder.Services.AddSingleton<IScheduleService, ScheduleService>();
		builder.Services.AddSingleton<ILiveMonitoringService, LiveMonitoringService>();
		builder.Services.AddSingleton<IReportService, ReportService>();
		builder.Services.AddSingleton<ITeacherSessionService, TeacherSessionService>();
		builder.Services.AddSingleton<IStudentPortalService, StudentPortalService>();
		builder.Services.AddSingleton<ILeaveRequestService, LeaveRequestService>();
		builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
		builder.Services.AddSingleton<IAuditService, AuditService>();
		builder.Services.AddSingleton<IRoomService, RoomService>();
		builder.Services.AddSingleton<IExportService, ExportService>();
		builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
		builder.Services.AddSingleton<IStudentExcuseService, StudentExcuseService>();
		builder.Services.AddSingleton<IAttendanceSettingsService, AttendanceSettingsService>();
		builder.Services.AddSingleton<IAtRiskService, AtRiskService>();
		builder.Services.AddSingleton<ISf2ReportService, Sf2ReportService>();
		builder.Services.AddSingleton<IDataEncryptionService, DataEncryptionService>();
		builder.Services.AddSingleton<IEncryptionDemoService, EncryptionDemoService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	/// <summary>Encryption:DataKey from the bundled appsettings.json, or null when absent.</summary>
	private static string? ReadConfiguredDataKey()
	{
		try
		{
			using var stream = Task.Run(() => FileSystem.OpenAppPackageFileAsync("appsettings.json")).GetAwaiter().GetResult();
			using var json = JsonDocument.Parse(stream);
			return json.RootElement.TryGetProperty("Encryption", out var section)
				&& section.TryGetProperty("DataKey", out var key) ? key.GetString() : null;
		}
		catch (Exception ex) when (ex is FileNotFoundException or JsonException)
		{
			return null;
		}
	}
}
