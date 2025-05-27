using System;
using System.Linq;
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using Microsoft.Office.Tools;
using DockPosition = Microsoft.Office.Core.MsoCTPDockPosition;
using System.Windows;
using Microsoft.VisualBasic;
using Control = System.Windows.Controls.Control;
using System.Windows.Media;
using WpfLib.Helpers;
using System.Windows.Threading;

namespace KleiKodesh.Helpers
{
    public static class WpfTaskPane
    {
        public static Dispatcher Dispatcher { get; private set; }
        public static CustomTaskPane Create<T>(T wpfControl, string title, int defaultWidth) where T : UIElement
        {
            try
            {
                if (!ExistingPane<T>(out var taskpane) && wpfControl is Control control)
                {
                    Dispatcher = control.Dispatcher;
                    string type = typeof(T).ToString();

                    var host = WpfHost(control);
                    taskpane = Globals.ThisAddIn.CustomTaskPanes.Add(host, title ?? typeof(T).Name);
                    SetWidth(taskpane, host, control, type, defaultWidth);
                    SetDockPostion(taskpane, type);

                    var document = Globals.Factory.GetVstoObject(Globals.ThisAddIn.Application.ActiveDocument);
                    document.CloseEvent += () => { Globals.ThisAddIn.CustomTaskPanes.Remove(taskpane); };

                    SetWpfColor(host, control);
                }

                taskpane.Visible = true;
                return taskpane;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return null; 
            }
        }

        private static bool ExistingPane<T>(out CustomTaskPane taskPane) where T : UIElement
        {
            taskPane = Globals.ThisAddIn.CustomTaskPanes
                .OfType<CustomTaskPane>()
                .FirstOrDefault(pane =>
                    pane.Control is UserControl hostControl &&
                    hostControl.Controls.OfType<ElementHost>()
                        .Any(host => host.Child is T) &&
                    pane.Window == Globals.ThisAddIn.Application.ActiveWindow);

            return taskPane != null;
        }
        static UserControl WpfHost(Control wpfControl)
        {
            var elementHost = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = wpfControl
            };

            var hostControl = new UserControl();
            hostControl.Controls.Add(elementHost);
            hostControl.Tag = wpfControl;
            return hostControl;
        }
        static void SetWidth(CustomTaskPane taskPane, UserControl hostControl, Control control, string type, int defaultWidth)
        {
            taskPane.Width = int.Parse(Interaction.GetSetting(AppDomain.CurrentDomain.ToString(), type, "TaskPaneWidth", defaultWidth.ToString()));

            control.Loaded += (s, e) =>
            {
                double actualWidth = control.RenderSize.Width;
                if (actualWidth > 0)
                {
                    int adjustedWidth = ConvertToScreenPixels(actualWidth) + 50;
                    taskPane.Width = Math.Max(adjustedWidth, hostControl.Width);
                }
            };

            hostControl.Disposed += (s, e) =>
            {
                try {  Interaction.SaveSetting(AppDomain.CurrentDomain.ToString(), type, "TaskPaneWidth", taskPane.Width.ToString()); } catch { }
            };
                
        }

        static void SetDockPostion(CustomTaskPane taskPane, string type)
        {
            var defaultPos = LocaleDictionary.IsRtl == true ? DockPosition.msoCTPDockPositionRight : DockPosition.msoCTPDockPositionLeft;
            if (Enum.TryParse(Interaction.GetSetting(AppDomain.CurrentDomain.ToString(), type, "DockPosition" + type + LocaleDictionary.Locale, defaultPos.ToString()),
                         out DockPosition savedDockPosition))
                taskPane.DockPosition = savedDockPosition;
            else
                taskPane.DockPosition = DockPosition.msoCTPDockPositionFloating;


            taskPane.DockPositionChanged += (sender, e) =>
                Interaction.SaveSetting(AppDomain.CurrentDomain.ToString(), type, "DockPosition" + type + LocaleDictionary.Locale, taskPane.DockPosition.ToString());
        }

        static void SetWpfColor(UserControl sidePanel, Control control)
        {
            try
            {
                Globals.ThisAddIn.officeThemeWatcher.OnThemeChanged();
            }
            catch
            { 
                var foreColor = sidePanel.ForeColor;
                var forefixedColor = Color.FromArgb(foreColor.A, foreColor.B, foreColor.G, foreColor.R);
                ThemeHelper.ForeGround = new SolidColorBrush(Color.FromArgb(forefixedColor.A, forefixedColor.R, forefixedColor.G, forefixedColor.B));

                var backColor = sidePanel.BackColor;
                var backfixedColor = Color.FromArgb(backColor.A, backColor.B, backColor.G, backColor.R);
                ThemeHelper.BackGround = new SolidColorBrush(Color.FromArgb(backfixedColor.A, backfixedColor.R, backfixedColor.G, backfixedColor.B));
            }
        }

        private static int ConvertToScreenPixels(double wpfUnits)
        {
            using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                float dpiX = graphics.DpiX; // Get the DPI of the screen
                return (int)(wpfUnits * (dpiX / 96.0)); // Convert WPF DIPs to screen pixels
            }
        }
    }
}
