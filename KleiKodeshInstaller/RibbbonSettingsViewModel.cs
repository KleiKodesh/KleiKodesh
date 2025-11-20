using System;
using System.ComponentModel;
using System.IO;

namespace KleiKodeshInstaller
{
    public class RibbbonSettingsViewModel
    {
        private static readonly string IniPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KleiKodesh", "RibbonSettings.csv");

        public SettingsModel ShowOtzarnik { get; } = new SettingsModel();
        public SettingsModel ShowZayit { get; } = new SettingsModel();
        public SettingsModel ShowWebSites { get; } = new SettingsModel();
        public SettingsModel ShowHebrewBooks { get; } = new SettingsModel();
        public SettingsModel ShowTypesetting { get; } = new SettingsModel();

        public RibbbonSettingsViewModel()
        {
            ShowOtzarnik.PropertyChanged += (s, _) => { Save();  };
            ShowZayit.PropertyChanged += (s, _) => { Save(); };
            ShowWebSites.PropertyChanged += (s, _) => { Save();  };
            ShowHebrewBooks.PropertyChanged += (s, _) => { Save(); };
            ShowTypesetting.PropertyChanged += (s, _) => { Save();};
            Load();
        }

        public void Load()
        {
            if (!File.Exists(IniPath)) return;
            var t = typeof(RibbbonSettingsViewModel);
            foreach (var l in File.ReadAllLines(IniPath))
            {
                var p = l.Split(','); if (p.Length != 3) continue;
                var prop = t.GetProperty(p[0]);
                var m = prop?.GetValue(this) as SettingsModel;
                if (m == null) continue;
                if (bool.TryParse(p[1], out var v)) m.IsVisible = v;
                if (bool.TryParse(p[2], out var d)) m.IsDefault = d;
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(IniPath));
            using (var w = new StreamWriter(IniPath))
                foreach (var p in typeof(RibbbonSettingsViewModel).GetProperties())
                {
                    if (p.PropertyType != typeof(SettingsModel)) continue;
                    var m = p.GetValue(this) as SettingsModel;
                    w.WriteLine($"{p.Name},{m.IsVisible},{m.IsDefault}");
                }
        }

        public string GetDefaultSettingKey()
        {
            if (ShowOtzarnik.IsDefault && ShowOtzarnik.IsVisible) return "Otzarnik";
            if (ShowZayit.IsDefault && ShowZayit.IsVisible) return "Zayit";
            if (ShowWebSites.IsDefault && ShowWebSites.IsVisible) return "WebSites";
            if (ShowHebrewBooks.IsDefault && ShowHebrewBooks.IsVisible) return "HebrewBooks";
            if (ShowTypesetting.IsDefault && ShowHebrewBooks.IsVisible) return "Typesetting";
            return null;
        }

        public int VisibleCount()
        {
            int count = 0;
            if (ShowOtzarnik.IsVisible) count++;
            if (ShowZayit.IsVisible) count++;
            if (ShowWebSites.IsVisible) count++;
            if (ShowHebrewBooks.IsVisible) count++;
            if (ShowTypesetting.IsVisible) count++;
            return count;
        }

        public class SettingsModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged(string n)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
            }

            private bool _isVisible = true, _isDefault;
            public bool IsVisible { get => _isVisible; set { if (_isVisible != value) { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); } } }
            public bool IsDefault { get => _isDefault; set { if (_isDefault != value) { _isDefault = value; OnPropertyChanged(nameof(IsDefault)); } } }
        }
    }
}
