using System.ComponentModel;
using System.Linq;
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
		if (context.HideoutItemRemaining.Count > 0)
		{
			Label hideoutTitleLabel = new Label();
			hideoutTitleLabel.Content = "Hideout: " + context.HideoutRemaining + "x";
			HideoutItemList.Children.Add(hideoutTitleLabel);

			var sortedHideoutItemRemaining = context.HideoutItemRemaining;
			foreach (var hideoutItemRemaining in sortedHideoutItemRemaining)
			{
				var label = new Label();
				label.Content = "[" + hideoutItemRemaining.Level + "] " + hideoutItemRemaining.Name + " " + hideoutItemRemaining.ItemCount + "x";
				HideoutItemList.Children.Add(label);
			}
		}


		TaskItemList.Children.Clear();
		if (context.TaskItemRemaining.Count > 0)
		{
			Label hideoutTitleLabel = new Label();
			hideoutTitleLabel.Content = "Tasks: " + context.TaskRemaining + "x";
			TaskItemList.Children.Add(hideoutTitleLabel);

			var sortedTaskItemRemaining = context.TaskItemRemaining.OrderBy(x => x.Task.Trader.Name).ThenBy(x => x.Task.MinPlayerLevel);
			foreach (var taskItemRemaining in sortedTaskItemRemaining)
			{
				var label = new Label();
				label.Content = "[" + taskItemRemaining.Task.MinPlayerLevel + "] " + taskItemRemaining.Task.Trader.Name + " => " + taskItemRemaining.Task.Name + " " + taskItemRemaining.ItemCount + "x";
				TaskItemList.Children.Add(label);
			}
		}


		BarterItemList.Children.Clear();
		if (context.LastItem.BartersUsing.Count > 0)
		{
			Label hideoutTitleLabel = new Label();
			hideoutTitleLabel.Content = "Barters";
			BarterItemList.Children.Add(hideoutTitleLabel);

			var item = context.LastItem;
			var sortedBarterItems = item.BartersUsing.OrderBy(x => x.Trader.Name).ThenBy(x => x.Level);
			foreach (var barter in sortedBarterItems)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("[" + barter.Level + "] ");
				sb.Append(barter.Trader.Name);
				sb.Append(" => ");

				for (var i = 0; i < barter.RequiredItems.Count; i++)
				{
					var requiredItem = barter.RequiredItems.ElementAt(i);
					sb.Append(requiredItem.Item.Name).Append(" ").Append(requiredItem.Count).Append("x");

					if (i < barter.RequiredItems.Count - 1)
					{
						sb.Append(" + ");
					}
				}

				var rewardItem = barter.RequiredItems.First();
				sb.Append(" == ").Append(rewardItem.Item.Name).Append(" ").Append(rewardItem.Count).Append("x");

				var label = new Label();
				label.Content = sb.ToString();
				BarterItemList.Children.Add(label);
			}
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
