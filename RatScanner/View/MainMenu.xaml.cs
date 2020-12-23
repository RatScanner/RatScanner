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
		
		public int lgInt;
		internal MainMenu(int lgIndex)
		{
			InitializeComponent();
			lgInt = lgIndex;
			DataContext = new MainWindowVM(RatScannerMain.Instance);

			switch (lgInt)
			{
				default:
					lgInt = 0;
					break;
				case 0:
					TranslateEN();
					break;
				case 3:
					TranslateFR();
					break;
			}
			
		}

		private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start("explorer.exe", e.Uri.ToString());
			e.Handled = true;
		}

		private void OpenSettingsWindow(object sender, RoutedEventArgs e)
		{
			PageSwitcher.Instance.Navigate(new Settings(lgInt));
		}

		public void UtilizeState(object state)
		{
			throw new System.NotImplementedException();
		}

		

		private void TranslateEN()
		{
			PriceLabel.Content = "Price";
			AvgDayPriceLabel.Content = "Average Day Price";
			AvgWeekPriceLabel.Content = "Average Week Price";
			PricePerSlotLabel.Content = "Price per Slot";

			TraderLabel.Content = "Trader";
			BuyPriceLabel.Content = "Buy Price";

			LinksLabel.Content = "Links";

			UpdatedLabel.Content = "Updated"; 
		}

		private void TranslateFR()
		{
			PriceLabel.Content = "Prix";
			AvgDayPriceLabel.Content = "Prix moyen - Journée";
			AvgWeekPriceLabel.Content = "Prix moyen - Semaine";
			PricePerSlotLabel.Content = "Prix par case";

			TraderLabel.Content = "Marchand"; 
			BuyPriceLabel.Content = "Prix d'achat";

			LinksLabel.Content = "Liens";

			UpdatedLabel.Content = "Dernière MaJ";
		}


		public void OnClose() { }

	
	}
}
