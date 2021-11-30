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

namespace RatScanner.View
{
	/// <summary>
	/// Interaction logic for BlazorUI.xaml
	/// </summary>
	public partial class BlazorUI : UserControl, ISwitchable
	{
		public BlazorUI()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddBlazorWebView();
			serviceCollection.AddMudServices();
			Resources.Add("services", serviceCollection.BuildServiceProvider());

			InitializeComponent();
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

		private void OpenSettingsWindow(object sender, RoutedEventArgs e)
		{
			PageSwitcher.Instance.Navigate(new Settings());
		}

		public void UtilizeState(object state)
		{
			throw new NotImplementedException();
		}

		public void OnOpen()
		{
			DataContext = new MainWindowVM(RatScannerMain.Instance);
			UpdateElements();
		}

		public void OnClose() { }
	}
}
