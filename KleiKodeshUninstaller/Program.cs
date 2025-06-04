using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace KleiKodeshUninstaller
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                string appName = "כלי קודש";
                string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "KleiKodesh");
                string addinKeyPath = @"Software\Microsoft\Office\Word\Addins\" + appName;
                string uninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName;

                // Remove registry keys (64-bit and 32-bit)
                RemoveRegistryKeys(addinKeyPath, uninstallKeyPath);

                // Delete installed files
                if (Directory.Exists(installPath))
                {
                    Directory.Delete(installPath, true);
                }

                MessageBox.Show("ההסרה הסתיימה בהצלחה", "הודעה");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"אירעה שגיאה: {ex.Message}", "שגיאה");
            }
        }

        static void RemoveRegistryKeys(string addinKeyPath, string uninstallKeyPath)
        {
            // 64-bit
            using (RegistryKey key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                key64.DeleteSubKeyTree(addinKeyPath, false);
                key64.DeleteSubKeyTree(uninstallKeyPath, false);
            }

            // 32-bit
            using (RegistryKey key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                key32.DeleteSubKeyTree(addinKeyPath, false);
                key32.DeleteSubKeyTree(uninstallKeyPath, false);
            }
        }
    }
}
