using RatScanner.View;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shell;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace RatScanner;

/// <summary>
/// Interaction logic for PageSwitcher.xaml
/// </summary>
public partial class PageSwitcher : Window {
	private NotifyIcon _notifyIcon = null!;
	private readonly ContextMenuStrip _contextMenuStrip = new();

	public static PageSwitcher Instance { get => field ??= new PageSwitcher(); private set; } = null!;

	private UserControl? activeControl;

	private const int WM_NCLBUTTONDOWN = 0x00A1;
	private const int HTCAPTION = 2;
	private const int HTBOTTOMRIGHT = 17;

	[LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
	private static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	[LibraryImport("user32.dll", EntryPoint = "ReleaseCapture")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool ReleaseCapture();

	public PageSwitcher() {
		try {
			Instance = this;
			RatConfig.LoadConfig();

			InitializeComponent();
			ResetWindowSize();
			Navigate(BlazorUI.Instance);
			AddJumpList();
			AddTrayIcon();

			if (RatConfig.LastWindowPositionX != int.MinValue || RatConfig.LastWindowPositionY != int.MinValue) {
				Left = RatConfig.LastWindowPositionX;
				Top = RatConfig.LastWindowPositionY;
			}

			// Restore window size if saved
			if (RatConfig.LastWindowWidth > 0 && RatConfig.LastWindowHeight > 0) {
				Width = RatConfig.LastWindowWidth;
				Height = RatConfig.LastWindowHeight;
			}

			Topmost = RatConfig.AlwaysOnTop;
			if (RatConfig.LastWindowMode == RatConfig.WindowMode.Minimal) ShowMinimalUI();
		} catch (Exception e) {
			Logger.LogError(e.Message, e);
		}
	}

	internal void ResetWindowSize() {
		SizeToContent = SizeToContent.Manual;
		MinWidth = MinNormalWindowWidth;
		MinHeight = MinNormalWindowHeight;
	}

	// The minimal UI sizes itself to its content, so the window minimums have to
	// be cleared for it. They are re-applied for the full UI, which is fixed-size
	// and would otherwise collapse to nothing.
	private const double MinNormalWindowWidth = 800;
	private const double MinNormalWindowHeight = 500;

	internal void Navigate(UserControl nextControl, object? state = null) {
		if (nextControl is not ISwitchable) throw new ArgumentException("NextPage is not ISwitchable! " + nextControl.Name);

		if (activeControl != null) {
			var activeControlSwitchable = (ISwitchable)activeControl;
			activeControlSwitchable.OnClose();
		}

		ContentControl.Content = nextControl;
		activeControl = nextControl;

		var nextControlSwitchable = (ISwitchable)nextControl;
		if (state != null) nextControlSwitchable.UtilizeState(state);

		nextControlSwitchable.OnOpen();
	}

	public void MinimizeWindow() {
		WindowState = WindowState.Minimized;
	}

	public void StartDrag() {
		try {
			var hwnd = new WindowInteropHelper(this).EnsureHandle();
			_ = ReleaseCapture();
			_ = SendMessage(hwnd, WM_NCLBUTTONDOWN, HTCAPTION, IntPtr.Zero);
		} catch (Exception ex) {
			Logger.LogError("Failed to start window drag", ex);
		}
	}

	public void StartResize() {
		try {
			var hwnd = new WindowInteropHelper(this).EnsureHandle();
			_ = ReleaseCapture();
			_ = SendMessage(hwnd, WM_NCLBUTTONDOWN, HTBOTTOMRIGHT, IntPtr.Zero);
		} catch (Exception ex) {
			Logger.LogError("Failed to start window resize", ex);
		}
	}

	protected override void OnStateChanged(EventArgs e) {
		if (RatConfig.MinimizeToTray && WindowState == WindowState.Minimized) Hide();

		base.OnStateChanged(e);
	}

	protected override void OnClosed(EventArgs e) {
		if (_notifyIcon != null) {
			_notifyIcon.Visible = false;
			_notifyIcon.Dispose();
		}

		base.OnClosed(e);
		ExitApplication();
	}

	private static void AddJumpList() {
		JumpTask showUITask = new() {
			Title = "Show UI",
			Arguments = "/showUI",
			Description = "Opens the main interface of RatScanner",
			IconResourcePath = Environment.ProcessPath,
			ApplicationPath = Environment.ProcessPath,

		};

		JumpTask showMinimalUITask = new() {
			Title = "Show Minimal UI",
			Arguments = "/showMinimalUI",
			Description = "Opens the minimal interface of RatScanner",
			IconResourcePath = Environment.ProcessPath,
			ApplicationPath = Environment.ProcessPath,
		};

		JumpTask showOverlayTask = new() {
			Title = "Show Overlay",
			Arguments = "/showOverlay",
			Description = "Opens the interactive overlay of RatScanner",
			IconResourcePath = Environment.ProcessPath,
			ApplicationPath = Environment.ProcessPath,
		};

		JumpList jumpList = new();
		jumpList.JumpItems.Add(showUITask);
		jumpList.JumpItems.Add(showMinimalUITask);
		jumpList.JumpItems.Add(showOverlayTask);
		jumpList.ShowFrequentCategory = false;
		jumpList.ShowRecentCategory = false;

		JumpList.SetJumpList(Application.Current, jumpList);
	}

	[MemberNotNull(nameof(_notifyIcon))]
	private void AddTrayIcon() {
		_notifyIcon = new NotifyIcon {
			Text = "Show",
			Visible = true,
			Icon = Properties.Resources.RatLogoSmall,
		};

		_ = _contextMenuStrip.Items.Add("Show UI", null, OnContextMenuShowUI);
		_ = _contextMenuStrip.Items.Add("Show Minimal UI", null, OnContextMenuShowMinimalUI);
		_ = _contextMenuStrip.Items.Add("Show Overlay", null, OnContextMenuShowOverlay);
		_ = _contextMenuStrip.Items.Add("Exit", null, OnContextMenuExitApplication);

		_notifyIcon.ContextMenuStrip = _contextMenuStrip;

		_notifyIcon.MouseClick += (sender, e) => {
			if (e.Button == System.Windows.Forms.MouseButtons.Left) {
				Show();
				WindowState = WindowState.Normal;
			}
		};
	}

	private void OnContextMenuShowOverlay(object? sender, EventArgs e) => ShowOverlay();
	private void OnContextMenuShowUI(object? sender, EventArgs e) => ShowUI();
	private void OnContextMenuShowMinimalUI(object? sender, EventArgs e) => ShowMinimalUI();
	private void OnContextMenuExitApplication(object? sender, EventArgs e) => ExitApplication();

	internal static void ShowOverlay() {
		BlazorUI.BlazorInteractableOverlay.ShowOverlay();
	}

	internal void ShowUI() {
		RatConfig.LastWindowMode = RatConfig.WindowMode.Normal;
		ResetWindowSize();

		// Switching back from the content-sized minimal UI leaves the window at the
		// minimal UI's dimensions, so restore the full UI's size.
		if (RatConfig.LastWindowWidth > 0 && RatConfig.LastWindowHeight > 0) {
			Width = RatConfig.LastWindowWidth;
			Height = RatConfig.LastWindowHeight;
		} else {
			Width = MinNormalWindowWidth;
			Height = MinNormalWindowHeight;
		}

		Navigate(BlazorUI.Instance);
	}

	internal void ShowMinimalUI() {
		RatConfig.LastWindowMode = RatConfig.WindowMode.Minimal;
		MinWidth = 0;
		MinHeight = 0;
		SizeToContent = SizeToContent.WidthAndHeight;
		Navigate(MinimalMenu.Instance);
	}

	internal void ExitApplication() {
		RatConfig.LastWindowPositionX = (int)Left;
		RatConfig.LastWindowPositionY = (int)Top;

		// Only the full UI has a meaningful size. While the minimal UI is showing the
		// window tracks its content, so persisting Width/Height here would shrink the
		// restored main window on the next launch.
		if (RatConfig.LastWindowMode == RatConfig.WindowMode.Normal) {
			RatConfig.LastWindowWidth = (int)Width;
			RatConfig.LastWindowHeight = (int)Height;
		}

		RatConfig.SaveConfig();
		Application.Current.Shutdown();
	}

	private void OnTitleBarMouseDown(object? sender, MouseButtonEventArgs e) {
		if (e.ChangedButton == MouseButton.Left) DragMove();
	}

	private void OnTitleBarMinimize(object? sender, RoutedEventArgs e) {
		RatConfig.LastWindowMode = RatConfig.WindowMode.Minimized;
		WindowState = WindowState.Minimized;
	}

	private void OnTitleBarMinimal(object? sender, RoutedEventArgs e) => ShowMinimalUI();

	private void OnTitleBarClose(object? sender, RoutedEventArgs e) {
		Close();
	}
}
