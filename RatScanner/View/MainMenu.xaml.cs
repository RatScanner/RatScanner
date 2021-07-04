using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using RatScanner.ViewModel;

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for MainMenuControl.xaml
	/// </summary>
	internal partial class MainMenu : UserControl, ISwitchable
	{
		internal MainMenu()
		{
			InitializeComponent();
		}

		private void UpdateElements()
		{
			const Visibility v = Visibility.Visible;
			const Visibility c = Visibility.Collapsed;

			var showTeam = RatConfig.Tracking.TarkovTracker.Enable;
			showTeam = showTeam && RatConfig.Tracking.TarkovTracker.ShowTeam;
			TeammateQHTrackingDisplay.Visibility = showTeam ? v : c;

			if (RatConfig.Tracking.TarkovTracker.ShowTeam)
			{
				var count = RatScannerMain.Instance.TarkovTrackerDB.TeammateCount;
				PageSwitcher.Instance.Height = PageSwitcher.DefaultHeight + ((float)count * 21.1) + 30;
			}
		}

		private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			var psi = new ProcessStartInfo
			{
				FileName = e.Uri.ToString(),
				UseShellExecute = true,
			};
			Process.Start(psi);
			e.Handled = true;
		}

		private void OpenSettingsWindow(object sender, RoutedEventArgs e)
		{
			PageSwitcher.Instance.Navigate(new Settings());
		}

		public void UtilizeState(object state)
		{
			throw new NotImplementedException();
		}

		public void OnOpen()
		{
			DataContext = new MainWindowVM(RatScannerMain.Instance);
			UpdateElements();
		}

		public void OnClose() { }
	}
}
