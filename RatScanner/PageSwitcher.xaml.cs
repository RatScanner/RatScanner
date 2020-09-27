using System;
using System.Windows;
using System.Windows.Controls;
using RatScanner.View;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace RatScanner
{
	/// <summary>
	/// Interaction logic for PageSwitcher.xaml
	/// </summary>
	public partial class PageSwitcher : Window
	{
		private NotifyIcon _notifyIcon;

		private static PageSwitcher _instance;
		public static PageSwitcher Instance => _instance ??= new PageSwitcher();

		public PageSwitcher()
		{
			try
			{
				_instance = this;

				InitializeComponent();

				Navigate(new MainMenu());

				AddTrayIcon();
				Topmost = RatConfig.AlwaysOnTop;

				(new MinimalWindow { DataContext = DataContext }).Show();
			}
			catch (Exception e)
			{
				Logger.LogError(e.Message, e);
			}
		}

		internal void Navigate(UserControl nextPage)
		{
			ContentControl.Content = nextPage;
		}

		internal void Navigate(UserControl nextPage, object state)
		{
			ContentControl.Content = nextPage;

			if (nextPage is ISwitchable s) s.UtilizeState(state);
			else
			{
				throw new ArgumentException("NextPage is not ISwitchable! " + nextPage.Name);
			}
		}

		protected override void OnStateChanged(EventArgs e)
		{
			if (RatConfig.MinimizeToTray && WindowState == WindowState.Minimized) Hide();

			base.OnStateChanged(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			_notifyIcon.Visible = false;
			_notifyIcon.Dispose();

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
	}
}
