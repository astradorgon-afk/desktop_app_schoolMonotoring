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

		var connectionString = "Server=localhost;Database=School_sms;User Id=root;Password=root;";

		builder.Services.AddDbContextFactory<SchoolDbContext>(options =>
			options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

		builder.Services.AddSingleton<IUserService, UserService>();
		builder.Services.AddSingleton<IAuthService, AuthService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
