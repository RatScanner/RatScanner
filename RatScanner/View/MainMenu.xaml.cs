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
			DataContext = new MainWindowVM(RatScannerMain.Instance);
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

		private void CheckLanguage()
		{
			switch (lgCombo.SelectedItem)
			{
				default:
					 lgCombo.Tag = 0;
				break;					
				case "FR":
						TranslateFR();
				break;
			}
		}

		private void TranslateFR()
		{

		}


		public void OnClose() { }

	}
}
