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

		window.Created += (s, e) =>
		{
#if WINDOWS
			var nativeWindow = (Microsoft.UI.Xaml.Window)window.Handler!.PlatformView!;
			var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
			var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
			var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
			if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
				presenter.Maximize();
#endif
		};

		return window;
	}
}
