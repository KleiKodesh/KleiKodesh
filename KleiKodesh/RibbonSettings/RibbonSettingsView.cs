using Office = Microsoft.Office.Core;

namespace KleiKodesh.RibbonSettings
{
    public class RibbonSettingsView : WebViewControl
    {
        public RibbonSettingsView(Office.IRibbonUI ribbon)
        {
            RibbonSettingsManager.Ribbon = ribbon;
            Navigate(RibbonSettingsManager.PagePath);
            RibbonSettingsManager.SettingsChanged += LoadSettings;
        }

        // Called via: { "action": "call", "target": "LoadSettings" }
        public async void LoadSettings()
        {
            await SendAsJson(new { type = "load", lines = RibbonSettingsManager.Data });
        }

        // Called via: { "action": "set", "target": "Lines", "value": [...] }
        public string[] Lines
        {
            set
            {
                RibbonSettingsManager.SettingsChanged -= LoadSettings;
                RibbonSettingsManager.Data = value;
                RibbonSettingsManager.SettingsChanged += LoadSettings;
            }
        }
    }
}
