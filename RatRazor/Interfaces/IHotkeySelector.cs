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
	public interface IHotkeySelector : INotifyPropertyChanged
	{
		public void StartListening();
		public void StopListening();
		public string ToString();

		public IHotkey HotkeyInterface { get; }

		public string HotkeyString { get; }

		public bool Listening { get; }
	}
}
