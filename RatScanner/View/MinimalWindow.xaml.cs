using System.Windows;
using System.Windows.Input;

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
			UpdateElements();
		}

		private void UpdateElements()
		{
			const Visibility v = Visibility.Visible;
			const Visibility c = Visibility.Collapsed;

			NameDisplay.Visibility = RatConfig.MinUiShowName ? v : c;
			PriceDisplay.Visibility = RatConfig.MinUiShowPrice ? v : c;
			AvgDayPriceDisplay.Visibility = RatConfig.MinUiShowAvgDayPrice ? v : c;
			AvgWeekPriceDisplay.Visibility = RatConfig.MinUiShowAvgWeekPrice ? v : c;
			PricePerSlotDisplay.Visibility = RatConfig.MinUiShowPricePerSlot ? v : c;
			UpdatedDisplay.Visibility = RatConfig.MinUiShowUpdated ? v : c;
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left) DragMove();
		}
	}
}
