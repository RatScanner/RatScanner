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
			switch (lgCombo.SelectedIndex)
			{
				default:
					lgCombo.SelectedIndex = 0;
					break;
				case 0:
					PageSwitcher.Instance.Navigate(new Settings(0));
					break;
				case 1:
					PageSwitcher.Instance.Navigate(new Settings(1));
					break;
				case 2:
					PageSwitcher.Instance.Navigate(new Settings(2));
					break;
				case 3:
					PageSwitcher.Instance.Navigate(new Settings(3));
					break;
				case 4:
					PageSwitcher.Instance.Navigate(new Settings(4));
					break;
				case 5:
					PageSwitcher.Instance.Navigate(new Settings(5));
					break;
			}
			
			
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
				case 0:
					TranslateEN();
					break;
				case 3:
					TranslateFR();
				break;
			}
		}

		private void TranslateEN()
		{
			///MainMenu
			PriceLabel.Content = "Price";
			AvgDayPriceLabel.Content = "Average Day Price";
			AvgWeekPriceLabel.Content = "Average Week Price";
			PricePerSlotLabel.Content = "Price per Slot";

			TraderLabel.Content = "Trader";
			BuyPriceLabel.Content = "Buy Price";

			LinksLabel.Content = "Links";

			UpdatedLabel.Content = "Updated";
			LanguageLabel.Content = "Language";
		}

		private void TranslateFR()
		{
			// MainMenu
			PriceLabel.Content = "Prix";
			AvgDayPriceLabel.Content = "Prix Moyen - Journée";
			AvgWeekPriceLabel.Content = "Prix Moyen - Semaine";
			PricePerSlotLabel.Content = "Prix par Case";

			TraderLabel.Content = "Marchand";
			BuyPriceLabel.Content = "Prix d'achat";

			LinksLabel.Content = "Liens";

			UpdatedLabel.Content = "Dernière MaJ";
			LanguageLabel.Content = "Langue";

			//Settings
			
		}

		private void lgCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CheckLanguage();
			
		}

		public void OnClose() { }

	
	}
}
