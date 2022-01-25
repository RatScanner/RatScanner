using System.ComponentModel;

namespace RatRazor.Interfaces;

public interface ISettingsUI : INotifyPropertyChanged
{
	public bool EnableNameScan { get; set; }
	public int NameScanLanguage { get; set; }

	public bool EnableIconScan { get; set; }
	public bool ScanRotatedIcons { get; set; }
	public bool UseCachedIcons { get; set; }
	public IHotkey IconScanHotkey { get; set; }

	public string ToolTipDuration { get; set; }
	public int ToolTipMilli { get; set; }

	public bool ShowName { get; set; }
	public bool ShowAvgDayPrice { get; set; }
	public bool ShowPricePerSlot { get; set; }
	public bool ShowTraderPrice { get; set; }
	public bool ShowTraderMaxPrice { get; set; }
	public bool ShowUpdated { get; set; }
	public bool ShowQuestHideoutTracker { get; set; }
	public bool ShowQuestHideoutTeamTracker { get; set; }
	public int Opacity { get; set; }

	public int ScreenWidth { get; set; }
	public int ScreenHeight { get; set; }
	public bool MinimizeToTray { get; set; }
	public bool AlwaysOnTop { get; set; }
	public bool LogDebug { get; set; }

	// Progress Tracking Settings
	public bool ShowNonFIRNeeds { get; set; }

	// TarkovTracker Specific Tracking Settings
	public string TarkovTrackerToken { get; set; }

	public bool ShowTarkovTrackerTeam { get; set; }

	public void SaveSettings();

	public void LoadSettings();
}
