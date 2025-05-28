using KleiKodesh.Helpers;
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
        private Office.IRibbonUI ribbon;

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
        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
        }

        void LoadSettings()
        {
            LocaleDictionary.UseOfficeLocale(Globals.ThisAddIn.Application, AppDomain.CurrentDomain.BaseDirectory);
            UpdateHelper.Update("KleiKodesh", "KleiKodesh", "v1.5", "נמצאו עדכונים עבור כלי קודש בוורד, האם ברצונך להורידם כעת?");
            Otzarnik.Helpers.WdWpfWindowHelper.Application = Globals.ThisAddIn.Application;
        }

        public void button_Click(Office.IRibbonControl control)
        {
            switch (control.Id)
            {
                case "Otzarnik":
                    WpfTaskPane.Create(new OtzarnikView(), LocaleDictionary.Translate(control.Id), 600);
                    break;
                case "WebSites":
                    WpfTaskPane.Create(new WebSitesView(), LocaleDictionary.Translate(control.Id), 500);
                    break;
                case "HebrewBooks":
                    WpfTaskPane.Create(new HebrewBooksLib.HebrewBooksView(), LocaleDictionary.Translate(control.Id), 600);
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
            string translation = LocaleDictionary.Translate(control.Id);
            return translation;
        }

        public System.Drawing.Image getImage(Office.IRibbonControl control)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", control.Id + ".png");
                System.Drawing.Bitmap image = new System.Drawing.Bitmap(path);
                return image;
            }
            catch
            {
                return null;
            }
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
