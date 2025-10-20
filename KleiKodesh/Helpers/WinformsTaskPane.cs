using Microsoft.Office.Tools;
using DockPosition = Microsoft.Office.Core.MsoCTPDockPosition;
using Microsoft.VisualBasic;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using WpfLib.Helpers;

namespace KleiKodesh.Helpers
{
    public static class WinformsTaskPane
    {
        public static CustomTaskPane Show(UserControl userControl, string title = " ", int width = 600, bool isVisible = true)
        {
            try
            {
                var type = userControl.GetType();
                var panes = Globals.ThisAddIn.CustomTaskPanes;
                var window = Globals.ThisAddIn.Application.ActiveWindow;

                var pane = panes.Cast<CustomTaskPane>()
                    .FirstOrDefault(p => p.Control.GetType() == type && p.Window == window)
                    ?? panes.Add(userControl, title ?? type.Name);

                pane.Width = width;
                pane.Visible = isVisible;
                //pane.VisibleChanged += (_, __) => userControl.Visible = pane.Visible;

                AttachRemoveOnClose(pane, userControl);
                SetColor(userControl);
                SetDockPostion(pane, type.Name);
                SetWidth(pane, userControl, type.Name, width);

                return pane;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
                return null;
            }
        }

        private static void AttachRemoveOnClose(CustomTaskPane pane, UserControl userControl)
        {
            try
            {
                Globals.Factory.GetVstoObject(Globals.ThisAddIn.Application.ActiveDocument)
                    .CloseEvent += () =>
                    {
                        Globals.ThisAddIn.CustomTaskPanes.Remove(pane);
                        userControl.Dispose();
                    };
            }
            catch { /* Swallow errors silently */ }
        }

        static void SetDockPostion(CustomTaskPane taskPane, string type)
        {
            var defaultPos = LocaleDictionary.IsRtl == true ? DockPosition.msoCTPDockPositionRight : DockPosition.msoCTPDockPositionLeft;
            if (Enum.TryParse(Interaction.GetSetting(AppDomain.CurrentDomain.FriendlyName, type, "DockPosition" + type + LocaleDictionary.Locale, defaultPos.ToString()),
                         out DockPosition savedDockPosition))
                taskPane.DockPosition = savedDockPosition;
            else
                taskPane.DockPosition = DockPosition.msoCTPDockPositionFloating;


            taskPane.DockPositionChanged += (sender, e) =>
                Interaction.SaveSetting(AppDomain.CurrentDomain.FriendlyName, type, "DockPosition" + type + LocaleDictionary.Locale, taskPane.DockPosition.ToString());
        }

        static void SetWidth(CustomTaskPane taskPane, UserControl userControl, string type, int defaultWidth)
        {
            taskPane.Width = int.Parse(Interaction.GetSetting(AppDomain.CurrentDomain.FriendlyName, type, "TaskPaneWidth", defaultWidth.ToString()));

            userControl.SizeChanged += (s, e) =>
            {
                try { Interaction.SaveSetting(AppDomain.CurrentDomain.FriendlyName, type, "TaskPaneWidth", taskPane.Width.ToString()); } catch { }
            };

        }

        static void SetColor(UserControl userControl)
        {
            try
            {
                Globals.ThisAddIn.officeThemeWatcher.OnThemeChanged();
            }
            catch
            {
                var foreColor = userControl.ForeColor;
                var forefixedColor = Color.FromArgb(foreColor.A, foreColor.B, foreColor.G, foreColor.R);
                WebViewLib.ThemeManager.Theme.Foreground = Color.FromArgb(forefixedColor.A, forefixedColor.R, forefixedColor.G, forefixedColor.B);

                var backColor = userControl.BackColor;
                var backfixedColor = Color.FromArgb(backColor.A, backColor.B, backColor.G, backColor.R);
                WebViewLib.ThemeManager.Theme.Background = Color.FromArgb(backfixedColor.A, backfixedColor.R, backfixedColor.G, backfixedColor.B);
            }
        }
    }
}
