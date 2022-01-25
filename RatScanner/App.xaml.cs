using Microsoft.Win32;
using SingleInstanceCore;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace RatScanner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, ISingleInstance
{
	private const string webview2regKey =
		"HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		// Set current working directory to executable location
		Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

		SetupExceptionHandling();

		// Install webview2 runtime if it is not already
		var winLogonKey = Registry.GetValue(webview2regKey, "pv", null);
		if (winLogonKey == null) InstallWebview2Runtime();

		// Setup single instance mode
		var guid = "{a057bb64-c126-4ef4-a4ed-3037c2e7bc89}";
		var isFirstInstance = this.InitializeAsFirstInstance(guid);
		if (!isFirstInstance)
		{
			SingleInstance.Cleanup();
			Current.Shutdown(2);
		}
	}

	public void OnInstanceInvoked(string[] args)
	{
		Current.Dispatcher.Invoke(() =>
		{
			MainWindow.Activate();
			MainWindow.WindowState = WindowState.Normal;

			// Invert the topmost state twice to bring the window on
			// top if it wasnt previously or do nothing
			MainWindow.Topmost = !MainWindow.Topmost;
			MainWindow.Topmost = !MainWindow.Topmost;
		});
	}

	private void InstallWebview2Runtime()
	{
		using var client = new WebClient();
		client.DownloadFile("https://go.microsoft.com/fwlink/p/?LinkId=2124703", "MicrosoftEdgeWebview2Setup.exe");

		var startInfo = new ProcessStartInfo();
		startInfo.CreateNoWindow = false;
		startInfo.UseShellExecute = false;
		startInfo.FileName = "MicrosoftEdgeWebview2Setup.exe";
		startInfo.WindowStyle = ProcessWindowStyle.Hidden;
		startInfo.Arguments = "/silent /install";

		try
		{
			// Start the process with the info we specified.
			// Call WaitForExit and then the using statement will close.
			var exeProcess = Process.Start(startInfo);
			exeProcess.WaitForExit();
		}
		catch (Exception ex)
		{
			Logger.LogError("Could not install Webview2", ex);
		}

		try
		{
			File.Delete("MicrosoftEdgeWebview2Setup.exe");
		}
		catch { }
	}

	private void SetupExceptionHandling()
	{
		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
		{
			LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
		};

		DispatcherUnhandledException += (s, e) =>
		{
			LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
			e.Handled = true;
		};

		TaskScheduler.UnobservedTaskException += (s, e) =>
		{
			LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
			e.SetObserved();
		};
	}

	private void LogUnhandledException(Exception exception, string source)
	{
		var message = $"Unhandled exception ({source})";
		Logger.LogError(message, exception);
	}
}
