﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using KleiKodesh.Ribbon;
using KleiKodesh.Helpers;

namespace KleiKodesh
{
    public partial class ThisAddIn
    {
        public OfficeThemeWatcher officeThemeWatcher = new OfficeThemeWatcher();
        protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new KleiKodeshRibbon();
        }

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            officeThemeWatcher.Stop();
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
