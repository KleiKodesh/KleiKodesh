using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace KleiKodesh.Helpers
{
    public static class WdWpfWindowHelper
    {
        public static void WdShow(this Window window, bool removeContent = false)
        {
            SetWordWindowOwner(window, removeContent);
            window.Show();
        }

        public static void WdShowDialog(this Window window, bool removeContent = false)
        {
            SetWordWindowOwner(window, removeContent);
            window.ShowDialog();
        }

        //יש להוסיף שם מחלקה - את שם הפרוייקט של התוסף אחרת globals לא ייקלט
        public static void SetWordWindowOwner(Window window, bool removeContent = false)
        {
            try
            {
                object content = null;  // optional remove window content if nessecary for perfomance isssues
                if (removeContent && content != null)
                {
                    content = window.Content;
                    window.Content = null;
                }

                IntPtr wordWindowHandle = IntPtr.Zero;

                Microsoft.Office.Interop.Word.Window activeWindow = Globals.ThisAddIn.Application.ActiveWindow;
                wordWindowHandle = new IntPtr(activeWindow.Hwnd);

                WindowInteropHelper helper = new WindowInteropHelper(window);
                helper.Owner = wordWindowHandle;

                if (removeContent && content != null) { window.Content = content; }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SetOwner: {ex.Message}");
            }
        }
    }
}
