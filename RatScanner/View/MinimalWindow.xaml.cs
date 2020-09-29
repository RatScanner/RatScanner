using System.Windows;
using System.Windows.Input;
using RatScanner.ViewModel;
using RCMinUi = RatScanner.RatConfig.MinimalUi;

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for MinimalWindow.xaml
	/// </summary>
	internal partial class MinimalWindow : Window
	{
		internal MinimalWindow()
		{
			InitializeComponent();
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
			UpdatedDisplay.Visibility = RCMinUi.ShowUpdated ? v : c;
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left) DragMove();
		}
	}
}
