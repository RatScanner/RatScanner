using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Force.DeepCloner;
using RatScanner.ViewModel;
using Color = System.Drawing.Color;
using RcFloatingTooltip = RatScanner.RatConfig.FloatingTooltip;
using RichTextBox = System.Windows.Forms.RichTextBox;
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
		if (RcFloatingTooltip.IsEnabled && sender != null && sender is MenuVM vm)
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
		const Visibility v = Visibility.Visible;
		const Visibility c = Visibility.Collapsed;

		NameDisplay.Visibility = RcFloatingTooltip.ShowName ? v : c;
		AvgDayPriceDisplay.Visibility = RcFloatingTooltip.ShowAvgDayPrice ? v : c;
		PricePerSlotDisplay.Visibility = RcFloatingTooltip.ShowPricePerSlot ? v : c;
		TraderPriceDisplay.Visibility = RcFloatingTooltip.ShowTraderPrice ? v : c;
		UpdatedDisplay.Visibility = RcFloatingTooltip.ShowUpdated ? v : c;
		TaskItemList.Visibility = RcFloatingTooltip.ShowTasksInfo ? v : c;
		HideoutItems.Visibility = RcFloatingTooltip.ShowHideoutInfo ? v : c;
		BarterItemList.Visibility = RcFloatingTooltip.ShowBarterInfo ? v : c;
		CraftItemList.Visibility = RcFloatingTooltip.ShowCraftsInfo ? v : c;

		var mousePosition = UserActivityHelper.GetMousePosition();
		Instance.Top = mousePosition.Y;
		Instance.Left = mousePosition.X;

		MenuVM context = ((MenuVM)Instance.DataContext);

		TaskItemList.Children.Clear();
		if (context.TaskItemRemaining.Count > 0 && RcFloatingTooltip.ShowTasksInfo)
		{
			TaskItemList.Children.Add(new Separator());

			Label hideoutTitle = new Label();
			hideoutTitle.Foreground = Brushes.Orange;
			hideoutTitle.Content = "Tasks: " + context.TaskRemaining + "x";
			TaskItemList.Children.Add(hideoutTitle);

			var sortedTaskItemRemaining = context.TaskItemRemaining.OrderBy(x => x.Task.Trader.Name).ThenBy(x => x.Task.MinPlayerLevel);
			foreach (var taskItemRemaining in sortedTaskItemRemaining)
			{
				var label = new Label();
				label.Content = taskItemRemaining.Task.MinPlayerLevel + "] " + taskItemRemaining.Task.Trader.Name + " => " + taskItemRemaining.Task.Name + " " + taskItemRemaining.ItemCount + "x";
				TaskItemList.Children.Add(label);
			}
		}

		HideoutItems.Children.Clear();
		if (context.HideoutItemRemaining.Count > 0 && RcFloatingTooltip.ShowHideoutInfo)
		{
			HideoutItems.Children.Add(new Separator());

			Label hideoutTitle = new Label();
			hideoutTitle.Foreground = Brushes.Orange;
			hideoutTitle.Content = "Hideout: " + context.HideoutRemaining + "x";
			HideoutItems.Children.Add(hideoutTitle);

			var sortedHideoutItemRemaining = context.HideoutItemRemaining;
			foreach (var hideoutItemRemaining in sortedHideoutItemRemaining)
			{
				var label = new Label();
				label.Content = "[" + hideoutItemRemaining.Level + "] " + hideoutItemRemaining.Name + " " + hideoutItemRemaining.ItemCount + "x";
				HideoutItems.Children.Add(label);
			}
		}


		BarterItemList.Children.Clear();
		if (context.LastItem.BartersUsing.Count > 0 && RcFloatingTooltip.ShowBarterInfo)
		{
			BarterItemList.Children.Add(new Separator());

			Label barterTitle = new Label();
			barterTitle.Foreground = Brushes.Orange;
			barterTitle.Content = "Barters";
			BarterItemList.Children.Add(barterTitle);

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

				var rewardItem = barter.RewardItems.First();
				sb.Append(" == ").Append(rewardItem.Item.Name).Append(" ").Append(rewardItem.Count).Append("x");

				var label = new Label();
				label.Content = sb.ToString();
				BarterItemList.Children.Add(label);
			}
		}

		CraftItemList.Children.Clear();
		if (context.LastItem.CraftsUsing.Count > 0 && RcFloatingTooltip.ShowCraftsInfo)
		{
			CraftItemList.Children.Add(new Separator());

			Label craftTitle = new Label();
			craftTitle.Foreground = Brushes.Orange;
			craftTitle.Content = "Crafts";
			CraftItemList.Children.Add(craftTitle);

			var item = context.LastItem;
			var sortedCraftItem = item.CraftsUsing.OrderBy(x => x.Station.Name).ThenBy(x => x.Level);
			foreach (var craft in sortedCraftItem)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("[" + craft.Level + "] ");
				sb.Append(craft.Station.Name);
				sb.Append(" => ");

				for (var i = 0; i < craft.RequiredItems.Count; i++)
				{
					var requiredItem = craft.RequiredItems.ElementAt(i);
					sb.Append(requiredItem.Item.Name).Append(" ").Append(requiredItem.Count).Append("x");

					if (i < craft.RequiredItems.Count - 1)
					{
						sb.Append(" + ");
					}
				}

				var rewardItem = craft.RewardItems.First();
				sb.Append(" == ").Append(rewardItem.Item.Name).Append(" ").Append(rewardItem.Count).Append("x");

				var label = new Label();
				label.Content = sb.ToString();
				CraftItemList.Children.Add(label);
			}
		}

		((Window)_instance).Show();
	}

	private void UpdateElements()
	{
		const Visibility v = Visibility.Visible;
		const Visibility c = Visibility.Collapsed;
	}


	private void rtb_AppendText(Color color, string text, RichTextBox box)
	{
		// append the text to the RichTextBox control
		int start = box.TextLength;
		box.AppendText(text);
		int end = box.TextLength;

		// select the new text
		box.Select(start, end - start);
		// set the attributes of the new text
		box.SelectionColor = color;
		// unselect
		box.Select(end, 0);

		// only required for multi line text to scroll to the end
		box.ScrollToCaret();
	}
}
