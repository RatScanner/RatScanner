using RatEye;
using RatScanner.ViewModel;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using Label = System.Windows.Controls.Label;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using RcFloatingTooltip = RatScanner.RatConfig.FloatingTooltip;
using Timer = System.Windows.Forms.Timer;

namespace RatScanner.View;

public partial class FloatingTooltip : Window {
	private static FloatingTooltip _instance = null!;
	public static FloatingTooltip Instance => _instance ??= new FloatingTooltip();

	private const int MouseMovementUntilHide = 10;

	private Vector2? mouseStartPosition;
	private string LastItemName;
	private int ItemScansCount;

	public event PropertyChangedEventHandler PropertyChanged;

	Timer fadeTimer;

	private FloatingTooltip() {
		InitializeComponent();
		Hide();
		MenuVM dataContext = new(RatScannerMain.Instance);
		DataContext = dataContext;
		LastItemName = dataContext.LastItem.Name;
		ItemScansCount = dataContext.ItemScans.Count;
		dataContext.PropertyChanged += OnPropertyChanged;
		UpdateElements();
	}

	protected override void OnMouseLeave(MouseEventArgs e) {
		Instance.HideTooltip();
	}

	protected override void OnMouseMove(MouseEventArgs e) {
		Vector2 mousePosition = UserActivityHelper.GetMousePosition();
		if (mouseStartPosition == null) {
			mouseStartPosition = mousePosition;
		} else if (Int32.Abs((mousePosition.X - mouseStartPosition.X)) > MouseMovementUntilHide ||
				   Int32.Abs((mousePosition.Y - mouseStartPosition.Y)) > MouseMovementUntilHide) {
			Application.Current.Dispatcher.Invoke(() => {
				Instance.HideTooltip();
			});
		}
	}

	protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if (RcFloatingTooltip.Enable && sender is MenuVM vm) {
			if ((!vm.LastItem.Name.Equals(LastItemName) || vm.ItemScans.Count > ItemScansCount)) {
				LastItemName = vm.LastItem.Name;
				ItemScansCount = vm.ItemScans.Count;
				Application.Current.Dispatcher.Invoke(() => Instance.ShowTooltip());
			}
		}
	}

	private void HideTooltip() {
		mouseStartPosition = null;
		Hide();
	}

	private void ShowTooltip() {
		MenuVM context = ((MenuVM)Instance.DataContext);

		TaskItemList.Children.Clear();
		if (context.TaskItemRemaining.Count > 0 && RcFloatingTooltip.ShowTasksInfo) {
			TaskItemList.Children.Add(new Separator());

			Label hideoutTitle = new();
			hideoutTitle.Foreground = Brushes.Orange;
			hideoutTitle.Content = "Tasks: " + context.TaskRemaining + "x";
			TaskItemList.Children.Add(hideoutTitle);

			IOrderedEnumerable<TaskItemRemaining> sortedTaskItemRemaining = context.TaskItemRemaining.OrderBy(x => x.Task.Trader.Name).ThenBy(x => x.Task.MinPlayerLevel);
			foreach (TaskItemRemaining? taskItemRemaining in sortedTaskItemRemaining) {
				Label label = new();
				label.Content = "[" + taskItemRemaining.Task.MinPlayerLevel + "] " + taskItemRemaining.Task.Trader.Name + " => " + taskItemRemaining.Task.Name + " " + taskItemRemaining.ItemCount + "x";
				TaskItemList.Children.Add(label);
			}
		}

		HideoutItems.Children.Clear();
		if (context.HideoutItemRemaining.Count > 0 && RcFloatingTooltip.ShowHideoutInfo) {
			HideoutItems.Children.Add(new Separator());

			Label hideoutTitle = new();
			hideoutTitle.Foreground = Brushes.Orange;
			hideoutTitle.Content = "Hideout: " + context.HideoutRemaining + "x";
			HideoutItems.Children.Add(hideoutTitle);

			System.Collections.ObjectModel.ObservableCollection<HideoutItemRemaining> sortedHideoutItemRemaining = context.HideoutItemRemaining;
			foreach (HideoutItemRemaining hideoutItemRemaining in sortedHideoutItemRemaining) {
				Label label = new();
				label.Content = "[" + hideoutItemRemaining.Level + "] " + hideoutItemRemaining.Name + " " + hideoutItemRemaining.ItemCount + "x";
				HideoutItems.Children.Add(label);
			}
		}


		BarterItemList.Children.Clear();
		if (context.LastItem.BartersUsing.Count > 0 && RcFloatingTooltip.ShowBarterInfo) {
			BarterItemList.Children.Add(new Separator());

			Label barterTitle = new();
			barterTitle.Foreground = Brushes.Orange;
			barterTitle.Content = "Barters";
			BarterItemList.Children.Add(barterTitle);

			TarkovDev.GraphQL.Item item = context.LastItem;
			IOrderedEnumerable<TarkovDev.GraphQL.Barter?> sortedBarterItems = item.BartersUsing.OrderBy(x => x.Trader.Name).ThenBy(x => x.Level);
			foreach (TarkovDev.GraphQL.Barter? barter in sortedBarterItems) {
				StringBuilder sb = new();
				sb.Append("[" + barter.Level + "] ");
				sb.Append(barter.Trader.Name);
				sb.Append(" => ");

				for (int i = 0; i < barter.RequiredItems.Count; i++) {
					TarkovDev.GraphQL.ContainedItem? requiredItem = barter.RequiredItems.ElementAt(i);
					sb.Append(requiredItem.Item.Name).Append(" ").Append(requiredItem.Count).Append("x");

					if (i < barter.RequiredItems.Count - 1) {
						sb.Append(" + ");
					}
				}

				TarkovDev.GraphQL.ContainedItem? rewardItem = barter.RewardItems.First();
				sb.Append(" == ").Append(rewardItem.Item.Name).Append(" ").Append(rewardItem.Count).Append("x");

				Label label = new();
				label.Content = sb.ToString();
				BarterItemList.Children.Add(label);
			}
		}

		CraftItemList.Children.Clear();
		if (context.LastItem.CraftsUsing.Count > 0 && RcFloatingTooltip.ShowCraftsInfo) {
			CraftItemList.Children.Add(new Separator());

			Label craftTitle = new();
			craftTitle.Foreground = Brushes.Orange;
			craftTitle.Content = "Crafts";
			CraftItemList.Children.Add(craftTitle);

			TarkovDev.GraphQL.Item item = context.LastItem;
			IOrderedEnumerable<TarkovDev.GraphQL.Craft?> sortedCraftItem = item.CraftsUsing.OrderBy(x => x.Station.Name).ThenBy(x => x.Level);
			foreach (TarkovDev.GraphQL.Craft? craft in sortedCraftItem) {
				StringBuilder sb = new();
				sb.Append("[" + craft.Level + "] ");
				sb.Append(craft.Station.Name);
				sb.Append(" => ");

				for (int i = 0; i < craft.RequiredItems.Count; i++) {
					TarkovDev.GraphQL.ContainedItem? requiredItem = craft.RequiredItems.ElementAt(i);
					sb.Append(requiredItem.Item.Name).Append(" ").Append(requiredItem.Count).Append("x");

					if (i < craft.RequiredItems.Count - 1) {
						sb.Append(" + ");
					}
				}

				TarkovDev.GraphQL.ContainedItem? rewardItem = craft.RewardItems.First();
				sb.Append(" == ").Append(rewardItem.Item.Name).Append(" ").Append(rewardItem.Count).Append("x");

				Label label = new();
				label.Content = sb.ToString();
				CraftItemList.Children.Add(label);
			}
		}

		Vector2 mousePosition = UserActivityHelper.GetMousePosition();
		Instance.Top = Math.Min(RatConfig.ScreenHeight, mousePosition.Y - MouseMovementUntilHide + Instance.Height) - Instance.Height;
		Instance.Left = Math.Min(RatConfig.ScreenWidth, mousePosition.X - MouseMovementUntilHide + Instance.Width) - Instance.Width;

		Instance.Show();
	}

	public void UpdateElements() {
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
	}
}
