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
        // === Constants ===
        const string AppName = "כלי קודש";
        const string Version = "1.6";
        const string InstallFolderName = "KleiKodesh";
        const string ZipResourceName = "KleiKodeshInstaller.InstallerFiles.zip";
        const string VstoFileName = "KleiKodesh.vsto";
        const string UninstallExeName = "Uninstall.exe";

        static string InstallPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), InstallFolderName);

        static string AddinRegistryPath =>
            $@"Software\Microsoft\Office\Word\Addins\{AppName}";

        static string UninstallRegistryPath =>
            $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{AppName}";

        const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.Title + " " + Version;
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e) => Install();

        private void UnInstallButton_Click(object sender, RoutedEventArgs e) => Uninstall();

        void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        async void Install()
        {
            try
            {
                string basePath = InstallPath;

                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                await ExtractZipFromResourceAsync(ZipResourceName, basePath);

                string currentExe = Assembly.GetExecutingAssembly().Location;
                string uninstallExe = Path.Combine(basePath, UninstallExeName);
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
            // 64-bit
            using (RegistryKey key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey addinKey = key64.CreateSubKey(AddinRegistryPath))
                {
                    addinKey.SetValue("FriendlyName", AppName);
                    addinKey.SetValue("Manifest", $"file:///{installPath}\\{VstoFileName}|vstolocal");
                    addinKey.SetValue("LoadBehavior", 3, RegistryValueKind.DWord);
                }

                using (RegistryKey uninstallKey = key64.CreateSubKey(UninstallRegistryPath))
                {
                    uninstallKey.SetValue("DisplayName", $"{AppName} v{Version}");
                    uninstallKey.SetValue("UninstallString", $"{installPath}\\{UninstallExeName}");
                    uninstallKey.SetValue("InstallLocation", installPath);
                    uninstallKey.SetValue("DisplayVersion", Version);
                }
            }

            // 32-bit
            using (RegistryKey key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                using (RegistryKey addinKey = key32.CreateSubKey(AddinRegistryPath))
                {
                    addinKey.SetValue("FriendlyName", AppName);
                    addinKey.SetValue("Manifest", $"file:///{installPath}\\{VstoFileName}|vstolocal");
                    addinKey.SetValue("LoadBehavior", 3, RegistryValueKind.DWord);
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);

        void Uninstall()
        {
            string installPath = InstallPath;

            try
            {
                foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                {
                    using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    {
                        try { baseKey.DeleteSubKeyTree(UninstallRegistryPath, false); } catch { }
                        try { baseKey.DeleteSubKeyTree(AddinRegistryPath, false); } catch { }
                    }
                }

                try
                {
                    using (RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default))
                    {
                        currentUser.DeleteSubKeyTree(AddinRegistryPath, false);
                    }
                }
                catch { }

                string cmd = $"/C timeout /t 2 & rmdir /s /q \"{installPath}\"";
                var psi = new ProcessStartInfo("cmd.exe", cmd)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var proc = Process.Start(psi);
                proc.WaitForExit(5000);

                if (Directory.Exists(installPath))
                {
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