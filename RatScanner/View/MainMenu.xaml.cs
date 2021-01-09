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

		private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start("explorer.exe", e.Uri.ToString());
			e.Handled = true;
		}

		private void OpenSettingsWindow(object sender, RoutedEventArgs e)
		{
			PageSwitcher.Instance.Navigate(new Settings());
		}

		public void UtilizeState(object state)
		{
			throw new System.NotImplementedException();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			DataContext = new MainWindowVM(RatScannerMain.Instance);
		}

		public void OnClose() { }
	}
}
