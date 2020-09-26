using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;
using RatScanner.ViewModel;
using Application = System.Windows.Application;

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Settings _settings;
		private NotifyIcon _notifyIcon;

		public MainWindow()
		{
			try
			{
				InitializeComponent();
				var ratScanner = new RatScannerMain();
				DataContext = new MainWindowVM(ratScanner);
				AddTrayIcon();
				Topmost = RatConfig.AlwaysOnTop;
			}
			catch (Exception e)
			{
				Logger.LogError(e.Message, e);
			}
		}

		private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start("explorer.exe", e.Uri.ToString());
			e.Handled = true;
		}

		protected override void OnStateChanged(EventArgs e)
		{
			if (RatConfig.MinimizeToTray && WindowState == WindowState.Minimized) Hide();

			base.OnStateChanged(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			_notifyIcon.Visible = false;
			_notifyIcon.Dispose();

			base.OnClosed(e);
			Application.Current.Shutdown();
		}

		private void OpenSettingsWindow(object sender, RoutedEventArgs e)
		{
			_settings?.Close();
			_settings = new Settings();
			_settings.ShowDialog();

			// Apply changed settings which affect appearance
			Topmost = RatConfig.AlwaysOnTop;
		}

		private void AddTrayIcon()
		{
			_notifyIcon = new NotifyIcon
			{
				Text = "Show",
				Visible = true,
				Icon = Properties.Resources.RatLogoSmall,
			};

			_notifyIcon.Click += delegate
			{
				Show();
				WindowState = WindowState.Normal;
			};
		}
	}
}
