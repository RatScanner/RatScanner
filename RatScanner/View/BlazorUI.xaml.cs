using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using RatRazor.Interfaces;
using RatScanner.Controls;
using RatScanner.ViewModel;
using RatTracking;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using RatLib;

namespace RatScanner.View;

/// <summary>
/// Interaction logic for BlazorUI.xaml
/// </summary>
public partial class BlazorUI : UserControl, ISwitchable
{
	public HotkeySelector IconScanHotkeySelector { get; set; }
	public static BlazorOverlay BlazorOverlay { get; set; }

	public BlazorUI()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddBlazorWebView();
		serviceCollection.AddMudServices();
		RatConfig.LoadConfig();

		serviceCollection.AddSingleton<IRatScannerUI>(s => new MainWindowVM(RatScannerMain.Instance));

		var settingsVM = new SettingsVM();
		serviceCollection.AddSingleton<ISettingsUI>(s => settingsVM);

		IconScanHotkeySelector = new HotkeySelector();
		IconScanHotkeySelector.Hotkey = (Hotkey)settingsVM.IconScanHotkey;
		IconScanHotkeySelector.Width = 0;
		IconScanHotkeySelector.Height = 0;
		serviceCollection.AddSingleton<IHotkeySelector>(s => IconScanHotkeySelector);

		serviceCollection.AddSingleton<VirtualScreenOffset>(s =>
			new VirtualScreenOffset((int)SystemParameters.VirtualScreenLeft, (int)SystemParameters.VirtualScreenTop));

		serviceCollection.AddSingleton<TarkovTrackerDB>(s => RatScannerMain.Instance.TarkovTrackerDB);

		var serviceProvider = serviceCollection.BuildServiceProvider();

		Resources.Add("services", serviceProvider);

		BlazorOverlay ??= new BlazorOverlay(serviceProvider);
		BlazorOverlay.Show();

		InitializeComponent();
	}

	private void BlazorUI_Loaded(object sender, RoutedEventArgs e)
	{
		Panel.SetZIndex(IconScanHotkeySelector, 100);
		blazorUIGrid.Children.Add(IconScanHotkeySelector);
		blazorWebView.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
		blazorWebView.WebView.NavigationCompleted += WebView_Loaded;
		blazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
	}

	private void WebView_Loaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
	{
		// If we are running in a development/debugger mode, open dev tools to help out
		if (Debugger.IsAttached) blazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
	}

	private void CoreWebView_Loaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
	{
		blazorWebView.WebView.CoreWebView2.Navigate("https://0.0.0.0/app");
	}

	private void UpdateElements() { }

	private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		var psi = new ProcessStartInfo
		{
			FileName = e.Uri.ToString(),
			UseShellExecute = true,
		};
		Process.Start(psi);
		e.Handled = true;
	}

	public void UtilizeState(object state)
	{
		throw new NotImplementedException();
	}

	protected override void OnPreviewKeyDown(KeyEventArgs e) { }

	private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			PageSwitcher.Instance.DragMove();
			e.Handled = true;
		}
	}

	public void OnOpen()
	{
		UpdateElements();
	}

	public void OnClose() { }
}
