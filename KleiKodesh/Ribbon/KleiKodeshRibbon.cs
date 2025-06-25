using KleiKodesh.Helpers;
using Microsoft.Office.Core;
using Oztarnik.Main;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using WebSitesLib;
using WpfLib.Helpers;
using Office = Microsoft.Office.Core;

namespace KleiKodesh.Ribbon
{
    [ComVisible(true)]
    public class KleiKodeshRibbon : Office.IRibbonExtensibility
    {
        private IRibbonUI ribbon;

        public KleiKodeshRibbon()
        {

        }

        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("KleiKodesh.Ribbon.KleiKodeshRibbon.xml");
        }

        #endregion

        #region Ribbon Callbacks
        //Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226

        bool isLoaded;
        public void Ribbon_Load(IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
            RibbbonSettingsViewModelHost.Ribbon = ribbonUI;
        }

        void LoadSettings()
        {
            LocaleDictionary.UseOfficeLocale(Globals.ThisAddIn.Application, AppDomain.CurrentDomain.BaseDirectory);
            UpdateHelper.Update("KleiKodesh", "KleiKodesh", "v1.7.2", 1, "נמצאו עדכונים עבור כלי קודש בוורד, האם ברצונך להורידם כעת?");
            Oztarnik.Helpers.WdWpfWindowHelper.Application = Globals.ThisAddIn.Application;
            var taskPane = WpfTaskPane.Show(new SettingsView(), " ", 600, false);
        }


        public void button_Click(Office.IRibbonControl control)
        {
            if (control.Id == "Klei_Kodesh_Main")
                Execute(RibbbonSettingsViewModelHost.RibbbonSettings.GetDefaultSettingKey());
            else
                Execute(control.Id);
        }

        void Execute(string id)
        {
            switch (id)
            {
                case "Otzarnik":
                    WpfTaskPane.Show(new OtzarnikView(), LocaleDictionary.Translate(id), 600);
                    //WpfTaskPane.Create(new OtzarnikLib.UI.MainView(), LocaleDictionary.Translate(id), 600);
                    break;
                case "WebSites":
                    WpfTaskPane.Show(new WebSitesView(), LocaleDictionary.Translate(id), 500);
                    break;
                case "HebrewBooks":
                    WpfTaskPane.Show(new HebrewBooksLib.HebrewBooksView(), LocaleDictionary.Translate(id), 600);
                    break;
                case "Typesetting":
                    WpfTaskPane.Show(new DocSeferLib.DocSeferLibView(Globals.ThisAddIn.Application), LocaleDictionary.Translate(id), 500);
                    break;
                case "Settings":
                    WpfTaskPane.Show(new SettingsView(), LocaleDictionary.Translate(id), 600);
                    break;
            }
        }

        public string getLabel(Office.IRibbonControl control)
        {
            if (!isLoaded)
            {
                LoadSettings();
                isLoaded = true;
            }
            string translation = LocaleDictionary.Translate(control.Id.Replace("_Main", ""));
            return translation;
        }

        public System.Drawing.Image getImage(Office.IRibbonControl control)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", control.Id.Replace("_Main", "") + ".png");
                System.Drawing.Bitmap image = new System.Drawing.Bitmap(path);
                return image;
            }
            catch
            {
                return null;
            }
        }

        public bool getVisible(Office.IRibbonControl control)
        {
            var settings = RibbbonSettingsViewModelHost.RibbbonSettings;
            if (settings == null) return true;

            var property = settings.GetType().GetProperty($"Show{control.Id}");
            if (property == null) return true;

            var model = property.GetValue(settings) as RibbbonSettingsViewModel.SettingsModel;
            return model?.IsVisible ?? true;
        }


        #endregion

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
