using RatScanner.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RCMinUi = RatScanner.RatConfig.MinimalUi;

namespace RatScanner.View;

/// <summary>
/// Interaction logic for MinimalMenu.xaml
/// </summary>
public partial class MinimalMenu : UserControl, ISwitchable
{
	internal MinimalMenu()
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
		AvgDayPriceDisplay.Visibility = RCMinUi.ShowAvgDayPrice ? v : c;
		PricePerSlotDisplay.Visibility = RCMinUi.ShowPricePerSlot ? v : c;
		TraderPriceDisplay.Visibility = RCMinUi.ShowTraderPrice ? v : c;
		TraderMaxPriceDisplay.Visibility = RCMinUi.ShowTraderMaxPrice ? v : c;
		TrackingDisplay.Visibility = RCMinUi.ShowQuestHideoutTracker ? v : c;
		TeamTrackingDisplay.Visibility = RCMinUi.ShowQuestHideoutTeamTracker ? v : c;
		UpdatedDisplay.Visibility = RCMinUi.ShowUpdated ? v : c;
	}

	private void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left) PageSwitcher.Instance.DragMove();
	}

	private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		PageSwitcher.Instance.ResetWindowSize();
		PageSwitcher.Instance.SetBackgroundOpacity(1);
		PageSwitcher.Instance.ShowTitleBar();
		PageSwitcher.Instance.Navigate(new BlazorUI());
	}

	public void UtilizeState(object state)
	{
		throw new System.NotImplementedException();
	}

	public void OnOpen() { }

	public void OnClose() { }
}
