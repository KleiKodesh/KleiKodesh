using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace KleiKodeshInstaller
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e) => 
            Install();
        
        private void UnInstallButton_Click(object sender, RoutedEventArgs e) =>
            Uninstall();
       
        void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }


        async void Install()
        {
            try
            {
                string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "KleiKodesh");

                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                await ExtractZipFromResourceAsync("KleiKodeshInstaller.InstallerFiles.zip", basePath);
                
                string currentExe = Assembly.GetExecutingAssembly().Location;
                string uninstallExe = Path.Combine(basePath, "Uninstall.exe");
                File.Copy(currentExe, uninstallExe, true);

                RegisterAddin(basePath);

                MessageBox.Show("ההתקנה הסתיימה בהצלחה");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "שגיאה");
            }
        }


        private async Task ExtractZipFromResourceAsync(string resourceName, string outputFolder)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException("Resource not found: " + resourceName);

                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    int total = archive.Entries.Count;
                    int current = 0;

                    foreach (var entry in archive.Entries)
                    {
                        string fullPath = Path.Combine(outputFolder, entry.FullName);

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(fullPath);
                            continue;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                        using (var entryStream = entry.Open())
                        using (var fileStream = File.Create(fullPath))
                        {
                            await entryStream.CopyToAsync(fileStream);
                        }

                        current++;
                        double progressValue = (double)current / total * 100;

                        // Update UI from background thread
                        await Dispatcher.InvokeAsync(() =>
                        {
                            Progress.Value = progressValue;
                        });
                    }

                    await Dispatcher.InvokeAsync(() =>
                    {
                        Progress.Value = Progress.Maximum;
                    });
                }
            }
        }

        void RegisterAddin(string installPath)
        {
            string appName = "כלי קודש";
            string displayVersion = "1.5";
            string addinKeyPath = @"Software\Microsoft\Office\Word\Addins\" + appName;
            string uninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName;

            // 64-bit
            using (RegistryKey key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey addinKey = key64.CreateSubKey(addinKeyPath))
                {
                    addinKey.SetValue("FriendlyName", appName);
                    addinKey.SetValue("Manifest", $"file:///{installPath}\\KleiKodesh.vsto|vstolocal");
                    addinKey.SetValue("LoadBehavior", 3, RegistryValueKind.DWord);
                }

                using (RegistryKey uninstallKey = key64.CreateSubKey(uninstallKeyPath))
                {
                    uninstallKey.SetValue("DisplayName", $"{appName} v{displayVersion}");
                    uninstallKey.SetValue("UninstallString", $"{installPath}\\Uninstall.exe");
                    uninstallKey.SetValue("InstallLocation", installPath);
                    uninstallKey.SetValue("DisplayVersion", displayVersion);
                }
            }

            // 32-bit
            using (RegistryKey key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (RegistryKey addinKey = key32.CreateSubKey(addinKeyPath))
                {
                    addinKey.SetValue("FriendlyName", appName);
                    addinKey.SetValue("Manifest", $"file:///{installPath}\\KleiKodesh.vsto|vstolocal");
                    addinKey.SetValue("LoadBehavior", 3, RegistryValueKind.DWord);
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);

        const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;

        void Uninstall()
        {
            string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "KleiKodesh");
            string appName = "כלי קודש";
            string uninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName;
            string addinKeyPath = @"Software\Microsoft\Office\Word\Addins\" + appName;

            try
            {
                foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                {
                    using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    {
                        try { baseKey.DeleteSubKeyTree(uninstallKeyPath, false); } catch { }
                        try { baseKey.DeleteSubKeyTree(addinKeyPath, false); } catch { }
                    }
                }

                try
                {
                    using (RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
                    {
                        currentUser.DeleteSubKeyTree(addinKeyPath, false);
                    }
                }
                catch { }

                string cmd = $"/C timeout /t 2 & rmdir /s /q \"{installPath}\"";
                var psi = new System.Diagnostics.ProcessStartInfo("cmd.exe", cmd)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                var proc = System.Diagnostics.Process.Start(psi);
                proc.WaitForExit(5000); // wait a bit for deletion

                if (Directory.Exists(installPath))
                {
                    // Schedule delete on reboot if still exists
                    MoveFileEx(installPath, null, MOVEFILE_DELAY_UNTIL_REBOOT);
                }

                Close();
                MessageBox.Show("ההתקנה הוסרה");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uninstall failed: " + ex.Message);
            }
        }
    }
}
