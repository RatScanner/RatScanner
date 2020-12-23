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
			CheckLanguage();

			DataContext = new MainWindowVM(RatScannerMain.Instance);			
			//lgCombo.SelectedIndex = 0;
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

		public void CheckLanguage()
		{
			switch (lgCombo.SelectedIndex)
			{
				default:
					lgCombo.SelectedIndex = 0;
					break;
				case 3:
					TranslateFR();
				break;
			}
		}

		private void TranslateFR()
		{
			/// MainMenu
			PriceLabel.Content = "Prix";
			AvgDayPriceLabel.Content = "Prix Moyen - Journée";
			AvgWeekPriceLabel.Content = "Prix Moyen - Semaine";
			PricePerSlotLabel.Content = "Prix par Case";

			TraderLabel.Content = "Marchand";
			BuyPriceLabel.Content = "Prix d'achat";

			LinksLabel.Content = "Liens";

			UpdatedLabel.Content = "Dernière MaJ";
			LanguageLabel.Content = "Langue";

			
		}

		public void OnClose() { }

		private void lgCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CheckLanguage();
			
		}
	}
}
