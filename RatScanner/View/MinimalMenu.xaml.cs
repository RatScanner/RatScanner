using RatEye;
using RatScanner.ViewModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using RCMinUi = RatScanner.RatConfig.MinimalUi;
using UserControl = System.Windows.Controls.UserControl;

namespace RatScanner.View;

/// <summary>
/// Interaction logic for MinimalMenu.xaml
/// </summary>
public partial class MinimalMenu : UserControl, ISwitchable {
	private static MinimalMenu _instance = null!;
	public static MinimalMenu Instance => _instance ??= new MinimalMenu();

	private MinimalMenu() {
		InitializeComponent();
		DataContext = new MenuVM(RatScannerMain.Instance);
		UpdateElements();
	}

	private void UpdateElements() {
		const Visibility v = Visibility.Visible;
		const Visibility c = Visibility.Collapsed;

		NameDisplay.Visibility = RCMinUi.ShowName ? v : c;
		AvgDayPriceDisplay.Visibility = RCMinUi.ShowAvgDayPrice ? v : c;
		PricePerSlotDisplay.Visibility = RCMinUi.ShowPricePerSlot ? v : c;
		TraderPriceDisplay.Visibility = RCMinUi.ShowTraderPrice ? v : c;
		TrackingDisplay.Visibility = RCMinUi.ShowQuestHideoutTracker ? v : c;
		TeamTrackingDisplay.Visibility = RCMinUi.ShowQuestHideoutTeamTracker ? v : c;
		UpdatedDisplay.Visibility = RCMinUi.ShowUpdated ? v : c;
	}

	private void OnMouseDown(object? sender, MouseButtonEventArgs e) {
		if (e.ChangedButton == MouseButton.Left) PageSwitcher.Instance.DragMove();
	}

	private void OnMouseDoubleClick(object? sender, MouseButtonEventArgs e) {
		PageSwitcher.Instance.ShowUI();
	}

	public void UtilizeState(object state) {
		throw new System.NotImplementedException();
	}

	public void OnOpen() { }

	public void OnClose() { }

	private void OnSizeChanged(object? sender, SizeChangedEventArgs e) {
		PageSwitcher win = PageSwitcher.Instance;
		Vector2 center = new((int)(win.Left + win.Width / 2), (int)(win.Top + win.Height / 2));
		Screen screen = Screen.FromPoint(center);
		bool isOnLeftSide = center.X < screen.Bounds.X + screen.Bounds.Width / 2;
		if (isOnLeftSide) return;

		// Else make it grow to the left
		double delta = e.PreviousSize.Width - e.NewSize.Width;
		PageSwitcher.Instance.Left += delta;
	}

}
