using System;
using Microsoft.Win32;
using System.Windows.Media;
using System.Management;
using System.Security.Principal;
using WpfLib.Helpers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace KleiKodesh.Helpers
{
    

    public static class OfficeThemeCodes
    {
        public const int Colorful = 0;
        public const int DarkGray = 3;
        public const int Black = 4;
        public const int White = 5;
    }

    public class OfficeTheme
    {
        public int Code { get; set; }
        public Color BackgroundColor { get; set; }
        public Color TextColor { get; set; }
    }

    public class OfficeThemeWatcher
    {
        private ManagementEventWatcher watcher;

        public OfficeThemeWatcher()
        {
            StartWatcher();
        }

        private int GetOfficeVersion()
        {
            try
            {
                using (var rk = Registry.ClassesRoot.OpenSubKey(@"Word.Application\CurVer"))
                {
                    if (rk != null)
                    {
                        var versionString = rk.GetValue("") as string;
                        if (!string.IsNullOrEmpty(versionString))
                        {
                            var parts = versionString.Split('.');
                            var lastPart = parts[parts.Length - 1];
                            int result;
                            if (int.TryParse(lastPart, out result))
                            {
                                return result;
                            }
                        }
                    }
                }
            }
            catch { }
            return 0;
        }

        private int GetCurrentThemeCode()
        {
            try
            {
                var version = GetOfficeVersion().ToString("F1");
                using (var rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\" + version + @"\Common"))
                {
                    if (rk != null)
                    {
                        return Convert.ToInt32(rk.GetValue("UI Theme", OfficeThemeCodes.Colorful));
                    }
                }
            }
            catch { }

            return OfficeThemeCodes.Colorful;
        }

        public OfficeTheme GetCurrentTheme()
        {
            var theme = new OfficeTheme();
            theme.Code = GetCurrentThemeCode();

            bool isSystemInDarkMode = IsSystemInDarkMode();

            // Set default colors based on system theme
            theme.BackgroundColor = isSystemInDarkMode ? (Color)ColorConverter.ConvertFromString("#323130") : Colors.White;
            theme.TextColor = isSystemInDarkMode ? Colors.White : Colors.Black;

            switch (theme.Code)
            {
                case OfficeThemeCodes.Colorful:
                    //theme.BackgroundColor = (Color)ColorConverter.ConvertFromString("#E6E6E6"); //taskpane gray 
                    //theme.BackgroundColor = (Color)ColorConverter.ConvertFromString("#FFF3F3F3"); //ribbon gray
                    //theme.TextColor = (Color)ColorConverter.ConvertFromString("#2B579A"); // Office Blue
                    theme.BackgroundColor = Colors.White;
                    theme.TextColor = (Color)ColorConverter.ConvertFromString("#FF262626");
                    break;

                case OfficeThemeCodes.DarkGray:
                    theme.BackgroundColor = (Color)ColorConverter.ConvertFromString("#666666");
                    theme.TextColor = Colors.White;
                    break;

                case OfficeThemeCodes.Black:
                    theme.BackgroundColor = (Color)ColorConverter.ConvertFromString("#FF262626");
                    theme.TextColor = Colors.White;
                    break;

                case OfficeThemeCodes.White:
                    theme.BackgroundColor = Colors.White;
                    theme.TextColor = (Color)ColorConverter.ConvertFromString("#FF262626");
                    break;
            }

            return theme;
        }

        private bool IsSystemInDarkMode()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object appsUseLightTheme = key.GetValue("AppsUseLightTheme");
                        if (appsUseLightTheme != null && appsUseLightTheme is int)
                        {
                            return ((int)appsUseLightTheme) == 0; // 0 means dark mode
                        }
                    }
                    return false;
                }
            }
            catch { return false; }           
        }


        private void StartWatcher()
        {
            var version = GetOfficeVersion().ToString("F1");
            var keyPath = @"Software\Microsoft\Office\" + version + @"\Common";
            var valueName = "UI Theme";

            var userSid = WindowsIdentity.GetCurrent()?.User?.Value;
            if (string.IsNullOrEmpty(userSid))
                return;

            var query = new WqlEventQuery(string.Format(@"
                SELECT * FROM RegistryValueChangeEvent 
                WHERE Hive='HKEY_USERS' 
                AND KeyPath='{0}\\{1}' 
                AND ValueName='{2}'",
                userSid,
                keyPath.Replace(@"\", @"\\"),
                valueName
            ));

            watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += (_, __) => OnThemeChanged();
            watcher.Start();
        }

        public void OnThemeChanged()
        {
            var theme = GetCurrentTheme();
               WpfTaskPane.Dispatcher?.Invoke(() => {
                   ThemeHelper.Background = new SolidColorBrush(theme.BackgroundColor);
                   ThemeHelper.Foreground = new SolidColorBrush(theme.TextColor);
               });
        }


        public void Stop()
        {
            if (watcher != null)
            {
                watcher.Stop();
                watcher.Dispose();
                watcher = null;
            }
        }
    }
}
