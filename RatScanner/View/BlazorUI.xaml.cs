using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System.Diagnostics;
using RatScanner.ViewModel;
using RatRazor.Interfaces;
using Microsoft.Web.WebView2.Core;
using RatScanner.Controls;

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for BlazorUI.xaml
	/// </summary>
	public partial class BlazorUI : UserControl, ISwitchable
	{
		public HotkeySelector IconScanHotkeySelector { get; set; }

		public BlazorUI()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddBlazorWebView();
			serviceCollection.AddMudServices();
			serviceCollection.AddSingleton<IRatScannerUI>(s => new MainWindowVM(RatScannerMain.Instance));
			var settingsVM = new SettingsVM();
			serviceCollection.AddSingleton<ISettingsUI>(s => settingsVM);
			IconScanHotkeySelector = new HotkeySelector();
			IconScanHotkeySelector.Hotkey = settingsVM.IconScanHotkey;
			IconScanHotkeySelector.Width = 0;
			IconScanHotkeySelector.Height = 0;
			serviceCollection.AddSingleton<IHotkeySelector>(s => IconScanHotkeySelector);
			Resources.Add("services", serviceCollection.BuildServiceProvider());

			InitializeComponent();
		}

		void BlazorUI_Loaded(object sender, RoutedEventArgs e)
		{
			Panel.SetZIndex(IconScanHotkeySelector, 100);
			blazorUIGrid.Children.Add(IconScanHotkeySelector);
			blazorWebView.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
			blazorWebView.WebView.NavigationCompleted += WebView_Loaded;
		}

		void WebView_Loaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
		{
			// If we are running in a development/debugger mode, open dev tools to help out
			if(Debugger.IsAttached)
			{
				blazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
				
			}
			
		}

		private void UpdateElements()
		{
			const Visibility v = Visibility.Visible;
			const Visibility c = Visibility.Collapsed;

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
