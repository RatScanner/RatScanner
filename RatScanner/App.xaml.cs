using System;
using System.Threading.Tasks;
using System.Windows;

namespace RatScanner
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			SetupExceptionHandling();
		}

		private void SetupExceptionHandling()
		{
			AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				LogUnhandledException((Exception) e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
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

		private async void Application_Exit(object sender, ExitEventArgs e)
		{
			await RatScannerMain.Instance.SaveWishlist();
		}
	}
}
