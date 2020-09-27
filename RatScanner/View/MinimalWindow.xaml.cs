using System;
using System.Collections.Generic;
using System.Text;
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
	/// Interaction logic for MinimalWindow.xaml
	/// </summary>
	internal partial class MinimalWindow : Window
	{
		internal MinimalWindow()
		{
			InitializeComponent();
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left) DragMove();
		}
	}
}
