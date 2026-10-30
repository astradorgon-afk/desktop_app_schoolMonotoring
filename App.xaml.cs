namespace AttendanceUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new MainPage()) { Title = "AttendanceUI" };

#if WINDOWS
		window.Created += (s, e) => MaximizeWindow(window);
#endif

		return window;
	}

#if WINDOWS
	/// <summary>Opens the main window maximized on Windows.</summary>
	/// <remarks>
	/// The app window can only be reached through the native window, so this has to
	/// wait for <see cref="Window.Created"/>. That hook fires before the native
	/// handle is guaranteed to be usable, which makes the AppWindow lookup below an
	/// intermittent failure: it either works or fails with E_FAIL, and because an
	/// exception here escapes through a WinUI event handler it takes the whole
	/// process down — the "dotnet run does nothing" crash, recorded by Windows only
	/// as APPCRASH 0xc000027b / HRESULT 0x80004005 inside Microsoft.UI.Xaml.dll.
	/// A window that opens un-maximized is a cosmetic loss, so any failure is
	/// logged and swallowed instead of being allowed to kill startup.
	/// </remarks>
	private static void MaximizeWindow(Window window)
	{
		try
		{
			if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window nativeWindow)
				return;

			var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
			var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
			var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

			if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
				presenter.Maximize();
		}
		catch (Exception ex)
		{
			AttendanceUI.WinUI.StartupLog.Write("MaximizeWindow", ex);
		}
	}
#endif
}
