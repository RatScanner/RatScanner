using RatScanner.View;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace RatScanner;

/// <summary>
/// Interaction logic for PageSwitcher.xaml
/// </summary>
public partial class PageSwitcher : Window
{
	public const int DefaultWidth = 280;
	public const int DefaultHeight = 450;

	private NotifyIcon _notifyIcon;

	private static PageSwitcher _instance;
	public static PageSwitcher Instance => _instance ??= new PageSwitcher();

	private UserControl activeControl;

	public PageSwitcher()
	{
		try
		{
			_instance = this;

			InitializeComponent();
			ResetWindowSize();
			Navigate(new BlazorUI());
			AddTrayIcon();

			Topmost = RatConfig.AlwaysOnTop;
		}
		catch (Exception e)
		{
			Logger.LogError(e.Message, e);
		}
	}

	internal void ResetWindowSize()
	{
		SizeToContent = SizeToContent.Manual;
		Width = DefaultWidth;
		Height = DefaultHeight;

		// Avoid window stretching when using minimal menu
		MaxWidth = DefaultWidth;
	}

	internal void Navigate(UserControl nextControl, object state = null)
	{
		if (!(nextControl is ISwitchable)) throw new ArgumentException("NextPage is not ISwitchable! " + nextControl.Name);

		if (activeControl != null)
		{
			var activeControlSwitchable = (ISwitchable)activeControl;
			activeControlSwitchable.OnClose();
		}

		ContentControl.Content = nextControl;
		activeControl = nextControl;

		var nextControlSwitchable = (ISwitchable)nextControl;
		if (state != null) nextControlSwitchable.UtilizeState(state);

		nextControlSwitchable.OnOpen();
	}

	protected override void OnStateChanged(EventArgs e)
	{
		if (RatConfig.MinimizeToTray && WindowState == WindowState.Minimized) Hide();

		base.OnStateChanged(e);
	}

	protected override void OnClosed(EventArgs e)
	{
		if (_notifyIcon != null)
		{
			_notifyIcon.Visible = false;
			_notifyIcon.Dispose();
		}

		base.OnClosed(e);

		Application.Current.Shutdown();
	}

	private void AddTrayIcon()
	{
		_notifyIcon = new NotifyIcon
		{
			Text = "Show",
			Visible = true,
			Icon = Properties.Resources.RatLogoSmall,
		};

		_notifyIcon.Click += delegate
		{
			Show();
			WindowState = WindowState.Normal;
		};
	}

	private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left) DragMove();
	}

	private void OnTitleBarMinimize(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private void OnTitleBarMinimal(object sender, RoutedEventArgs e)
	{
		CollapseTitleBar();
		SizeToContent = SizeToContent.WidthAndHeight;
		SetBackgroundOpacity(RatConfig.MinimalUi.Opacity / 100f);
		Navigate(new MinimalMenu());
	}

	private void OnTitleBarClose(object sender, RoutedEventArgs e)
	{
		Close();
	}

	internal void CollapseTitleBar()
	{
		TitleBar.Visibility = Visibility.Collapsed;
	}

	internal void ShowTitleBar()
	{
		TitleBar.Visibility = Visibility.Visible;
	}

	internal void SetBackgroundOpacity(float opacity)
	{
		//return;
		Background.Opacity = Math.Clamp(opacity, 1f / 510f, 1f);
	}
}
