using SingleInstanceCore;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace RatScanner
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application, ISingleInstance
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			SetupExceptionHandling();

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
			Application.Current.Dispatcher.Invoke(() =>
			{
				MainWindow.Activate();
				MainWindow.WindowState = WindowState.Normal;

				// Invert the topmost state twice to bring the window on
				// top if it wasnt previously or do nothing
				MainWindow.Topmost = !MainWindow.Topmost;
				MainWindow.Topmost = !MainWindow.Topmost;
			});
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
			try
			{
				var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
				message = $"Unhandled exception in {assemblyName.Name} {RatConfig.Version}";
			}
			catch (Exception ex)
			{
				Logger.LogError("Exception in LogUnhandledException", ex);
			}
			finally
			{
				Logger.LogError(message, exception);
			}
		}
	}
}
