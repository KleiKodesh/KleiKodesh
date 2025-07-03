using KleiKodesh.Ribbon;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Office = Microsoft.Office.Core;


namespace KleiKodesh.RibbonSettings
{
    public static class RibbonSettingsManager
    {
        public static event Action SettingsChanged;
        public static void OnSettingsChanged() => SettingsChanged?.Invoke();


        public static Office.IRibbonUI Ribbon { get; set; }
        static string CurrentFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RibbonSettings");
        public static string PagePath => Path.Combine(CurrentFolder, "RibbonSettingsPage.html");
        public static string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "KleiKodesh",
            "RibbonSettings.csv");


        private static string[] _data;
        public static string[] Data  { get => GetData(); set => SetData(value); }

        static string[] GetData()
        {
            if (_data == null) _data = File.Exists(SettingsPath)
                        ? File.ReadAllLines(SettingsPath)
                        : Array.Empty<string>();
            return _data;
        }

        static void SetData(string[] value)
        {
            if (_data == value) return;
            _data = value;
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllLines(SettingsPath, value.Where(x => !string.IsNullOrWhiteSpace(x)));
            OnSettingsChanged();
            InvalidateControls(value);
        }
      
        static void InvalidateControls(string[] data)
        {
            foreach (string line in Data)
            {
                var parts = line.Split(',');
                if (parts.Length > 0) Ribbon?.InvalidateControl(parts[0].Trim());
            }
        }

        public static string GetDefaultSettingKey()
        {
            foreach (var line in Data)
            {
                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                var key = parts[0].Trim();
                var isVisible = parts[1].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                var isDefault = parts[2].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);

                if (isDefault && isVisible)
                    return key;
            }

            return null;
        }

        public static bool GetVisible(string id)
        {
            foreach (var line in Data)
            {
                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                var key = parts[0].Trim();
                if (!string.Equals(key, id, StringComparison.OrdinalIgnoreCase)) continue;

                var isVisible = parts[1].Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                return isVisible;
            }

            return true;
        }

    }
}
