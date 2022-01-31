using System.ComponentModel;

namespace RatRazor.Interfaces;

public interface IHotkey : INotifyPropertyChanged
{
	public string HotkeyString { get; }
}
