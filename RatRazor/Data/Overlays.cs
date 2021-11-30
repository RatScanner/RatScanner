using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
//using System.Windows.Forms;

namespace RatRazor.Data
{
	public class Overlays
	{
		//[DllImport("User32.dll")]
		//public static extern IntPtr GetDC(IntPtr hwnd);
		//[DllImport("User32.dll")]
		//public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);

		//public Form tempForm = null;

		public void ShowPopup()
		{
			//Form form = new Form();
			//form.Location = new Point(x, y);

			//IntPtr desktopPtr = GetDC(IntPtr.Zero);
			//Graphics g = Graphics.FromHdc(desktopPtr);

			//SolidBrush b = new SolidBrush(Color.White);
			//g.FillRectangle(b, new Rectangle(0, 0, 1920, 1080));

			//g.Dispose();
			//ReleaseDC(IntPtr.Zero, desktopPtr);

		}
	}
}
