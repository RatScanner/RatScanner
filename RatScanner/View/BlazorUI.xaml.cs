using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using RatLib;
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

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for BlazorUI.xaml
	/// </summary>
	public partial class BlazorUI : UserControl, ISwitchable
	{
		public HotkeySelector IconScanHotkeySelector { get; set; }
		public BlazorOverlay BlazorOverlay { get; set; }

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
			IconScanHotkeySelector.Hotkey = settingsVM.IconScanHotkey;
			IconScanHotkeySelector.Width = 0;
			IconScanHotkeySelector.Height = 0;
			serviceCollection.AddSingleton<IHotkeySelector>(s => IconScanHotkeySelector);
			serviceCollection.AddSingleton<VirtualScreenOffset>(s => new VirtualScreenOffset((int)SystemParameters.VirtualScreenLeft, (int)SystemParameters.VirtualScreenTop));
			serviceCollection.AddSingleton<TarkovTrackerDB>(s => RatScannerMain.Instance.TarkovTrackerDB);

			ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

			BlazorOverlay = new BlazorOverlay(serviceProvider);
			BlazorOverlay.Show();

			Resources.Add("services", serviceProvider);

			InitializeComponent();
		}

		void BlazorUI_Loaded(object sender, RoutedEventArgs e)
		{
			Panel.SetZIndex(IconScanHotkeySelector, 100);
			blazorUIGrid.Children.Add(IconScanHotkeySelector);
			blazorWebView.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
			blazorWebView.WebView.NavigationCompleted += WebView_Loaded;
			blazorWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
		}

		void WebView_Loaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
		{
			// If we are running in a development/debugger mode, open dev tools to help out
			if (Debugger.IsAttached)
			{
				blazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
			}
		}

		void CoreWebView_Loaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
		{
			blazorWebView.WebView.CoreWebView2.Navigate("https://0.0.0.0/app");
		}

		private void UpdateElements()
		{
		}

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

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			//Test
		}

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
			//DataContext = new MainWindowVM(RatScannerMain.Instance);
			UpdateElements();
			//
			//blazorWebView.Foreground.Opacity = Math.Clamp(10, 1f / 510f, 1f);
		}

		public void OnClose() { }
	}
}
