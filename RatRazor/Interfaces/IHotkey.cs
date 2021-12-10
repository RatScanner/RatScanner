using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.ComponentModel;

namespace RatRazor.Interfaces
{
	public interface IHotkey : INotifyPropertyChanged
	{
		public string HotkeyString { get; }

	}
}
