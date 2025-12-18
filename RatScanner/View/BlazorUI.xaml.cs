using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using MudBlazor.Services;
using RatScanner.Localization;
using RatScanner.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace RatScanner.View;

/// <summary>
/// Interaction logic for BlazorUI.xaml
/// </summary>
public partial class BlazorUI : UserControl, ISwitchable {
	private static BlazorUI _instance = null;
	public static BlazorUI Instance => _instance ??= new BlazorUI();

	public static BlazorOverlay BlazorOverlay { get; set; }
	public static BlazorInteractableOverlay BlazorInteractableOverlay { get; set; }

	private BlazorUI() {
		ServiceCollection serviceCollection = new();
		serviceCollection.AddWpfBlazorWebView();
		serviceCollection.AddMudServices();

		serviceCollection.AddSingleton<MenuVM>(s => new MenuVM(RatScannerMain.Instance));

		LocalizationService localizationService = new();
		serviceCollection.AddSingleton(localizationService);

		SettingsVM settingsVM = new(localizationService);
		serviceCollection.AddSingleton<SettingsVM>(s => settingsVM);

		System.Collections.Generic.IEnumerable<System.Drawing.Rectangle> bounds = System.Windows.Forms.Screen.AllScreens.Select(screen => screen.Bounds);
		int left = 0;
		int top = 0;
		foreach (System.Drawing.Rectangle bound in bounds) {
			if (bound.Left < left) left = bound.Left;
			if (bound.Top < top) top = bound.Top;
		}
		serviceCollection.AddSingleton<VirtualScreenOffset>(s => new VirtualScreenOffset(left, top));

		serviceCollection.AddSingleton<TarkovTrackerDB>(s => RatScannerMain.Instance.TarkovTrackerDB);

		ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

		Resources.Add("services", serviceProvider);

		BlazorOverlay ??= new BlazorOverlay(serviceProvider);
		BlazorOverlay.Show();

		BlazorInteractableOverlay ??= new BlazorInteractableOverlay(serviceProvider);

		InitializeComponent();
	}

	private void BlazorUI_Loaded(object? sender, RoutedEventArgs e) {
		blazorWebView.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
		blazorWebView.WebView.NavigationCompleted += WebView_Loaded;
		blazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
	}

	private void WebView_Loaded(object? sender, CoreWebView2NavigationCompletedEventArgs e) {
		// If we are running in a development/debugger mode, open dev tools to help out
		if (Debugger.IsAttached) blazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
	}

	private void CoreWebView_Loaded(object? sender, CoreWebView2InitializationCompletedEventArgs e) {
		blazorWebView.WebView.CoreWebView2.SetVirtualHostNameToFolderMapping("local.data", "Data", CoreWebView2HostResourceAccessKind.Allow);
		blazorWebView.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
		blazorWebView.WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
	}

	private void UpdateElements() { }

	private void HyperlinkRequestNavigate(object? sender, RequestNavigateEventArgs e) {
		ProcessStartInfo psi = new() {
			FileName = e.Uri.ToString(),
			UseShellExecute = true,
		};
		Process.Start(psi);
		e.Handled = true;
	}

	public void UtilizeState(object state) {
		throw new NotImplementedException();
	}

	protected override void OnPreviewKeyDown(KeyEventArgs e) { }

	private void OnPreviewMouseDown(object? sender, MouseButtonEventArgs e) {
		if (e.ChangedButton == MouseButton.Left) {
			PageSwitcher.Instance.DragMove();
			e.Handled = true;
		}
	}

	public void OnOpen() {
		UpdateElements();
	}

	public void OnClose() { }
}
