using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using RatScanner.ViewModel;
using RCMinUi = RatScanner.RatConfig.FloatingTooltip;
using Timer = System.Windows.Forms.Timer;
using UserControl = System.Windows.Controls.UserControl;

namespace RatScanner.View;

public partial class FloatingTooltip : Window
{
	private static FloatingTooltip _instance = null!;
	public static FloatingTooltip Instance => _instance ??= new FloatingTooltip();

	private string LastItemName;
	private int ItemScansCount;

	public event PropertyChangedEventHandler PropertyChanged;

	Timer fadeTimer;

	public FloatingTooltip()
	{
		InitializeComponent();
		Hide();
		var dataContext = new MenuVM(RatScannerMain.Instance);
		DataContext = dataContext;
		LastItemName = dataContext.LastItem.Name;
		ItemScansCount = dataContext.ItemScans.Count;

		UpdateElements();
		dataContext.PropertyChanged += OnPropertyChanged;
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		Application.Current.Dispatcher.Invoke(() => FloatingTooltip.Instance.Hide());
	}

	protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (RCMinUi.IsEnabled && sender != null && sender is MenuVM vm)
		{
			if ((!vm.LastItem.Name.Equals(LastItemName) || vm.ItemScans.Count > ItemScansCount))
			{
				LastItemName = vm.LastItem.Name;
				ItemScansCount = vm.ItemScans.Count;
				Application.Current.Dispatcher.Invoke(() => FloatingTooltip.Instance.Show());
			}
		}
	}

	public void Show()
	{
		var mousePosition = UserActivityHelper.GetMousePosition();
		Instance.Top = mousePosition.Y;
		Instance.Left = mousePosition.X;
		((Window)_instance).Show();
	}

	private void UpdateElements()
	{
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


	private void FrameworkElement_OnTargetUpdated(object? sender, DataTransferEventArgs e)
	{
		string test = "test";
	}
}
