using Microsoft.VisualBasic;
using System;
using System.ComponentModel;

namespace KleiKodeshInstaller
{
    public static class SettingsViewModel
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName) { StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));}

        const string AppName = "Otzaria";
        const string Section = "RibbonSettings";
        static int _updateInterval = int.TryParse(Interaction.GetSetting(AppName, Section, "UpdateInterval"), out int result) ? result : 0;
        public static int UpdateInterval
        {
            get => _updateInterval;
            set
            {
                if (_updateInterval != value)
                {
                    _updateInterval = value;
                    Interaction.SaveSetting(AppName, Section, "UpdateIterval", value.ToString());
                }
            }
        }

        public static SettingsModel ShowOtzarnik { get; } = new SettingsModel("ShowOtzarnik", true);
        public static SettingsModel ShowWebSites { get; } = new SettingsModel("ShowWebSites", false);
        public static SettingsModel ShowHebrewBooks { get; } = new SettingsModel("ShowHebrewBooks", false);

        public static string GetDefaultSettingKey()
        {
            if (ShowOtzarnik.IsDefault && ShowOtzarnik.IsVisible) return ShowOtzarnik.Key.Replace("Show", "");
            if (ShowWebSites.IsDefault && ShowWebSites.IsVisible) return ShowWebSites.Key.Replace("Show", "");
            if (ShowHebrewBooks.IsDefault && ShowHebrewBooks.IsVisible) return ShowHebrewBooks.Key.Replace("Show", "");
            return null;
        }

        public static int VisibleCount()
        {
            int count = 0;
            if (ShowOtzarnik.IsVisible) count++;
            if (ShowWebSites.IsVisible) count++;
            if (ShowHebrewBooks.IsVisible) count++;
            return count;
        }
    }

    public class SettingsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        const string AppName = "Otzaria";
        const string Section = "RibbonSettings";

        bool _isVisible = true;
        bool _isDefault;

        public bool IsVisible
        {
            get => _isVisible;
            set { if (value != _isVisible) { _isVisible = value; Commit(); } }
        }

        public bool IsDefault
        {
            get => _isDefault;
            set { if (value != _isDefault) { _isDefault = IsVisible ?  value : false; Commit(); } }
        }

        public string Key { get; private set; }

        public SettingsModel(string key, bool isdefault)
        {
            Key = key;
            var value = Interaction.GetSetting(AppName, Section, key);
            if (!string.IsNullOrEmpty(value))
            {
                var parts = value.Split(',');
                if (parts.Length == 2 &&
                    bool.TryParse(parts[0], out var isVisible) &&
                    bool.TryParse(parts[1], out var isDefault))
                {
                    _isVisible = isVisible;
                    _isDefault = isDefault;
                }
            }
            else
            {
                IsDefault = isdefault;
            }
        }

        void Commit()
        {
            string value = $"{_isVisible},{_isDefault}";
            Interaction.SaveSetting(AppName, Section, Key, value);
            OnPropertyChanged(nameof(IsVisible));
            OnPropertyChanged(nameof(IsDefault));
        }
    }
}
