using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RatScanner.ViewModel;
using RCMinUi = RatScanner.RatConfig.FloatingTooltip;
using Timer = System.Windows.Forms.Timer;

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
		Application.Current.Dispatcher.Invoke(() => Instance.Hide());
	}

	protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (RCMinUi.IsEnabled && sender != null && sender is MenuVM vm)
		{
			if ((!vm.LastItem.Name.Equals(LastItemName) || vm.ItemScans.Count > ItemScansCount))
			{
				LastItemName = vm.LastItem.Name;
				ItemScansCount = vm.ItemScans.Count;
				Application.Current.Dispatcher.Invoke(() => Instance.Show());
			}
		}
	}

	public void Show()
	{
		var mousePosition = UserActivityHelper.GetMousePosition();
		Instance.Top = mousePosition.Y;
		Instance.Left = mousePosition.X;

		MenuVM context = ((MenuVM)Instance.DataContext);

		HideoutItemList.Children.Clear();
		foreach (var hideoutItemRemaining in context.HideoutItemRemaining)
		{
			var label = new Label();
			label.Content = "[" + hideoutItemRemaining.Level + "] " + hideoutItemRemaining.Name + " " + hideoutItemRemaining.ItemCount + "x";
			HideoutItemList.Children.Add(label);
		}

		((Window)_instance).Show();
	}

	private void UpdateElements()
	{
		const Visibility v = Visibility.Visible;
		const Visibility c = Visibility.Collapsed;
	}


	public static string GetHideoutItemsString()
	{
		return "Hideout items lol";
	}
}
