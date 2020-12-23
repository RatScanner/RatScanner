using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RatScanner.ViewModel;
using RCMinUi = RatScanner.RatConfig.MinimalUi;

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for MinimalMenu.xaml
	/// </summary>
	public partial class MinimalMenu : UserControl, ISwitchable
	{
		public int lgInt;
		internal MinimalMenu(int lgIndex)
		{
			InitializeComponent();
			lgInt = lgIndex;

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

			DataContext = new MainWindowVM(RatScannerMain.Instance);
			UpdateElements();
		}

		private void UpdateElements()
		{
			const Visibility v = Visibility.Visible;
			const Visibility c = Visibility.Collapsed;

			NameDisplay.Visibility = RCMinUi.ShowName ? v : c;
			PriceDisplay.Visibility = RCMinUi.ShowPrice ? v : c;
			AvgDayPriceDisplay.Visibility = RCMinUi.ShowAvgDayPrice ? v : c;
			AvgWeekPriceDisplay.Visibility = RCMinUi.ShowAvgWeekPrice ? v : c;
			PricePerSlotDisplay.Visibility = RCMinUi.ShowPricePerSlot ? v : c;
			TraderPriceDisplay.Visibility = RCMinUi.ShowTraderPrice ? v : c;
			UpdatedDisplay.Visibility = RCMinUi.ShowUpdated ? v : c;
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left) PageSwitcher.Instance.DragMove();
		}

		private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			PageSwitcher.Instance.Navigate(new MainMenu(lgInt));
			PageSwitcher.Instance.ShowTitleBar();
			PageSwitcher.Instance.ResetWindowSize();
			PageSwitcher.Instance.SetBackgroundOpacity(1);
		}

		public void UtilizeState(object state)
		{
			throw new System.NotImplementedException();
		}

		private void TranslateEN()
		{
			MinNameLabel.Content = "Name";
			MinPriceLabel.Content = "Price";
			MinAvgDayPriceLabel.Content = "Avg. Day Price";
			MinAvgWeekPriceLabel.Content = "Avg. Week Price";
			MinPPSLabel.Content = "Price per Slot";

			MinUpdatedLabel.Content = "Updated";
		}

		private void TranslateFR()
		{
			MinNameLabel.Content = "Nom";
			MinPriceLabel.Content = "Prix";
			MinAvgDayPriceLabel.Content = "Prix moy. - Ajd";
			MinAvgWeekPriceLabel.Content = "Prix moy. - Sem";
			MinPPSLabel.Content = "Prix par case";

			MinUpdatedLabel.Content = "Dernière MaJ";
		}

		

		public void OnClose() { }
	}
}
