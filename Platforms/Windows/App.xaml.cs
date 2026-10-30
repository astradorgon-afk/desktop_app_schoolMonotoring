using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AttendanceUI.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		// An unhandled exception here kills the process before anything is printed,
		// and WinUI reports it as a bare "stowed exception" that names no user code.
		// Logging each escape route is what makes such a crash diagnosable.
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
		{
			if (e.ExceptionObject is Exception exception)
				StartupLog.Write("AppDomain.UnhandledException", exception);
		};

		TaskScheduler.UnobservedTaskException += (_, e) =>
			StartupLog.Write("TaskScheduler.UnobservedTaskException", e.Exception);

		this.UnhandledException += (_, e) =>
			StartupLog.Write("Application.UnhandledException", e.Exception);

		RedirectWebViewUserData();
		this.InitializeComponent();
	}

	protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
	{
		try
		{
			base.OnLaunched(args);
		}
		catch (Exception ex)
		{
			StartupLog.Write("OnLaunched", ex);
			throw;
		}
	}

	/// <summary>
	/// Moves WebView2's user-data folder out of the install directory.
	/// </summary>
	/// <remarks>
	/// By default WebView2 creates "&lt;exe name&gt;.WebView2" alongside the executable. That
	/// is fine when running from bin/ or a publish folder, but once the app is installed
	/// under Program Files a standard user cannot write there: WebView2 fails to
	/// initialise, the BlazorWebView never loads, and the window comes up blank with no
	/// error. Pointing it at LocalAppData — which is always per-user writable — is the
	/// supported fix. It must run before any WebView2 environment is created, so it
	/// happens at the top of the constructor.
	/// </remarks>
	private static void RedirectWebViewUserData()
	{
		const string UserDataFolderVariable = "WEBVIEW2_USER_DATA_FOLDER";

		// Respect an explicit override if one is already set (e.g. by a deployment script).
		if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(UserDataFolderVariable)))
			return;

		try
		{
			var folder = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"AttendanceUI",
				"WebView2");

			Directory.CreateDirectory(folder);
			Environment.SetEnvironmentVariable(UserDataFolderVariable, folder);
		}
		catch (Exception)
		{
			// If LocalAppData is unavailable, leave the default in place rather than
			// preventing startup — a writable install directory still works.
		}
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

