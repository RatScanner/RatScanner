using System.ComponentModel;

namespace RatScanner.ViewModel
{
    internal class SettingsVM : INotifyPropertyChanged
    {
        public string ToolTipDuration { get; set; }
        public bool EnableNameScan { get; set; }
        public bool EnableIconScan { get; set; }
        public bool ScanRotatedIcons { get; set; }
        public bool UseCachedIcons { get; set; }
        public int IconCacheSize => IconManager.GetIconCacheSize();
        public int IconScanModifier { get; set; }
        public int ScreenResolution { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool AlwaysOnTop { get; set; }
        public bool LogDebug { get; set; }

        internal SettingsVM()
        {
            ToolTipDuration = RatConfig.ToolTipDuration.ToString();
            EnableNameScan = RatConfig.EnableNameScan;
            EnableIconScan = RatConfig.EnableIconScan;
            ScanRotatedIcons = RatConfig.ScanRotatedIcons;
            UseCachedIcons = RatConfig.UseCachedIcons;
            IconScanModifier = RatConfig.ModifierKeyCode;
            ScreenResolution = (int)RatConfig.ScreenResolution;
            MinimizeToTray = RatConfig.MinimizeToTray;
            AlwaysOnTop = RatConfig.AlwaysOnTop;
            LogDebug = RatConfig.LogDebug;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
