using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for BlazorOverlay.xaml
	/// </summary>
	public partial class BlazorOverlay : Window
	{
		public BlazorOverlay(ServiceProvider serviceProvider)
		{
			Resources.Add("services", serviceProvider);

			InitializeComponent();
		}

		void BlazorOverlay_Loaded(object sender, RoutedEventArgs e)
		{
			blazorOverlayWebView.WebView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
			Left = SystemParameters.VirtualScreenLeft;
			Top = SystemParameters.VirtualScreenTop;
			Width = SystemParameters.VirtualScreenWidth;
			Height = SystemParameters.VirtualScreenHeight;
			blazorOverlayWebView.WebView.NavigationCompleted += WebView_Loaded;
			blazorOverlayWebView.WebView.CoreWebView2InitializationCompleted += CoreWebView_Loaded;
		}

		void WebView_Loaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
		{
			// If we are running in a development/debugger mode, open dev tools to help out
			if (Debugger.IsAttached)
			{
				blazorOverlayWebView.WebView.CoreWebView2.OpenDevToolsWindow();
			}

		}

		void CoreWebView_Loaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
		{
			//blazorOverlayWebView.WebView.CoreWebView2.Navigate("/overlay");
		}
	}
}
