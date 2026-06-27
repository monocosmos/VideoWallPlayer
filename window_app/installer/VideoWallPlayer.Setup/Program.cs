using Microsoft.Win32;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace VideoWallPlayer.Setup;

internal static class Program
{
    private const string UninstallArg = "/uninstall";
    private const string TempModeArg = "--temp";

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (!IsAdministrator())
        {
            RelaunchElevated(args);
            return;
        }

        var uninstall = args.Any(arg => string.Equals(arg, UninstallArg, StringComparison.OrdinalIgnoreCase));
        var tempMode = args.Any(arg => string.Equals(arg, TempModeArg, StringComparison.OrdinalIgnoreCase));

        if (uninstall && !tempMode && InstallerPaths.IsRunningFromInstallDirectory())
        {
            RelaunchUninstallerFromTemp();
            return;
        }

        Application.Run(new SetupForm(uninstall, tempMode));
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static void RelaunchElevated(string[] args)
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exePath))
        {
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = true,
            Verb = "runas"
        };

        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        try
        {
            Process.Start(startInfo);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            MessageBox.Show(
                "Kurulum icin yonetici izni gerekiyor.",
                "VideoWallPlayer Kurulum",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private static void RelaunchUninstallerFromTemp()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(exePath))
        {
            return;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), "VideoWallPlayer-uninstall-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var tempExe = Path.Combine(tempDir, "VideoWallPlayerSetup.exe");
        File.Copy(exePath, tempExe, overwrite: true);

        Process.Start(new ProcessStartInfo
        {
            FileName = tempExe,
            UseShellExecute = false,
            Arguments = $"{UninstallArg} {TempModeArg}"
        });
    }
}

internal sealed class SetupForm : Form
{
    private readonly bool _uninstall;
    private readonly bool _tempMode;
    private readonly Button _primaryButton = new();
    private readonly Button _cancelButton = new();
    private readonly Label _titleLabel = new();
    private readonly Label _bodyLabel = new();
    private readonly Label _statusLabel = new();
    private readonly ProgressBar _progressBar = new();
    private readonly CheckBox _desktopShortcutCheckBox = new();
    private readonly TextBox _installPathTextBox = new();

    public SetupForm(bool uninstall, bool tempMode)
    {
        _uninstall = uninstall;
        _tempMode = tempMode;

        Text = uninstall ? "VideoWallPlayer Kaldirma" : "VideoWallPlayer Kurulum";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(620, 420);
        BackColor = Color.FromArgb(18, 22, 30);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        BuildLayout();
    }

    private void BuildLayout()
    {
        var logo = new PictureBox
        {
            Image = Icon?.ToBitmap(),
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(28, 28),
            Size = new Size(72, 72)
        };

        _titleLabel.AutoSize = false;
        _titleLabel.Font = new Font("Segoe UI Semibold", 21F);
        _titleLabel.ForeColor = Color.White;
        _titleLabel.Location = new Point(118, 28);
        _titleLabel.Size = new Size(460, 42);
        _titleLabel.Text = _uninstall ? "VideoWallPlayer kaldiriliyor" : "VideoWallPlayer kuruluyor";

        _bodyLabel.AutoSize = false;
        _bodyLabel.ForeColor = Color.FromArgb(180, 233, 238);
        _bodyLabel.Location = new Point(120, 74);
        _bodyLabel.Size = new Size(450, 42);
        _bodyLabel.Text = _uninstall
            ? "Uygulama dosyalari, kisayollar ve Windows kaldirma kaydi temizlenecek."
            : "Tek paket, bagimsiz ve VLC/libVLC bilesenleri dahil kurulum.";

        var pathLabel = new Label
        {
            AutoSize = true,
            ForeColor = Color.FromArgb(184, 196, 214),
            Location = new Point(32, 142),
            Text = "Kurulum klasoru"
        };

        _installPathTextBox.Location = new Point(32, 168);
        _installPathTextBox.Size = new Size(556, 28);
        _installPathTextBox.Text = InstallerPaths.InstallDirectory;
        _installPathTextBox.ReadOnly = true;
        _installPathTextBox.BackColor = Color.FromArgb(12, 15, 22);
        _installPathTextBox.ForeColor = Color.White;
        _installPathTextBox.BorderStyle = BorderStyle.FixedSingle;

        _desktopShortcutCheckBox.AutoSize = true;
        _desktopShortcutCheckBox.Checked = true;
        _desktopShortcutCheckBox.ForeColor = Color.White;
        _desktopShortcutCheckBox.Location = new Point(32, 214);
        _desktopShortcutCheckBox.Text = "Masaustu kisayolu olustur";
        _desktopShortcutCheckBox.Visible = !_uninstall;

        _progressBar.Location = new Point(32, 270);
        _progressBar.Size = new Size(556, 18);
        _progressBar.Style = ProgressBarStyle.Continuous;

        _statusLabel.AutoSize = false;
        _statusLabel.ForeColor = Color.FromArgb(184, 196, 214);
        _statusLabel.Location = new Point(32, 304);
        _statusLabel.Size = new Size(556, 44);
        _statusLabel.Text = _uninstall ? "Kaldirma baslatilmaya hazir." : "Kurulum baslatilmaya hazir.";

        _primaryButton.Location = new Point(344, 360);
        _primaryButton.Size = new Size(118, 36);
        _primaryButton.Text = _uninstall ? "Kaldir" : "Kur";
        _primaryButton.BackColor = Color.FromArgb(0, 202, 218);
        _primaryButton.ForeColor = Color.FromArgb(4, 19, 27);
        _primaryButton.FlatStyle = FlatStyle.Flat;
        _primaryButton.FlatAppearance.BorderSize = 0;
        _primaryButton.Click += PrimaryButton_Click;

        _cancelButton.Location = new Point(470, 360);
        _cancelButton.Size = new Size(118, 36);
        _cancelButton.Text = "Iptal";
        _cancelButton.BackColor = Color.FromArgb(38, 48, 64);
        _cancelButton.ForeColor = Color.White;
        _cancelButton.FlatStyle = FlatStyle.Flat;
        _cancelButton.FlatAppearance.BorderColor = Color.FromArgb(58, 70, 88);
        _cancelButton.Click += (_, _) => Close();

        Controls.AddRange([
            logo,
            _titleLabel,
            _bodyLabel,
            pathLabel,
            _installPathTextBox,
            _desktopShortcutCheckBox,
            _progressBar,
            _statusLabel,
            _primaryButton,
            _cancelButton
        ]);
    }

    private async void PrimaryButton_Click(object? sender, EventArgs e) => await RunRequestedAction();

    private async Task RunRequestedAction()
    {
        SetBusy(true);

        try
        {
            if (_uninstall)
            {
                await Task.Run(Uninstall);
                SetProgress(100, "VideoWallPlayer kaldirildi.");
                _primaryButton.Text = "Tamam";
                _primaryButton.Click -= PrimaryButton_Click;
                _primaryButton.Click += (_, _) => Close();
                _primaryButton.Enabled = true;
                _cancelButton.Visible = false;
                return;
            }

            var createDesktopShortcut = _desktopShortcutCheckBox.Checked;
            await Task.Run(() => Install(createDesktopShortcut));
            SetProgress(100, "Kurulum tamamlandi. VideoWallPlayer kullanima hazir.");
            _primaryButton.Text = "Baslat";
            _primaryButton.Click -= PrimaryButton_Click;
            _primaryButton.Click += (_, _) => StartInstalledApp();
            _primaryButton.Enabled = true;
            _cancelButton.Text = "Kapat";
            _cancelButton.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            SetBusy(false);
        }
    }

    private void Install(bool createDesktopShortcut)
    {
        SetProgress(10, "Paket aciliyor...");
        Directory.CreateDirectory(InstallerPaths.InstallDirectory);

        var tempDir = Path.Combine(Path.GetTempPath(), "VideoWallPlayer-install-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var payloadZip = Path.Combine(tempDir, "payload.zip");
            ExtractPayload(payloadZip);

            SetProgress(35, "Uygulama dosyalari kopyalaniyor...");
            ZipFile.ExtractToDirectory(payloadZip, InstallerPaths.InstallDirectory, overwriteFiles: true);

            SetProgress(60, "Kaldirma bilgisi hazirlaniyor...");
            CopyInstallerToInstallDirectory();
            RegisterUninstaller();

            SetProgress(78, "Kisayollar olusturuluyor...");
            ShortcutService.CreateStartMenuShortcut();
            if (createDesktopShortcut)
            {
                ShortcutService.CreateDesktopShortcut();
            }
            else
            {
                ShortcutService.DeleteDesktopShortcut();
            }

            SetProgress(92, "Kurulum dogrulaniyor...");
            if (!File.Exists(InstallerPaths.AppExePath))
            {
                throw new FileNotFoundException("VideoWallPlayer.exe kurulum klasorunde bulunamadi.", InstallerPaths.AppExePath);
            }
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    private void Uninstall()
    {
        SetProgress(15, "Kisayollar kaldiriliyor...");
        ShortcutService.DeleteDesktopShortcut();
        ShortcutService.DeleteStartMenuShortcut();

        SetProgress(35, "Windows kaldirma kaydi temizleniyor...");
        Registry.LocalMachine.DeleteSubKeyTree(InstallerPaths.UninstallRegistryKey, throwOnMissingSubKey: false);

        SetProgress(58, "Uygulama dosyalari kaldiriliyor...");
        TryDeleteDirectory(InstallerPaths.InstallDirectory);

        if (_tempMode)
        {
            NativeMethods.DeleteFileOnReboot(Environment.ProcessPath);
        }

        SetProgress(90, "Kaldirma tamamlandi.");
    }

    private static void ExtractPayload(string destinationPath)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Payload.zip")
            ?? throw new InvalidOperationException("Kurulum paketi bozuk: gomulu payload bulunamadi.");
        using var output = File.Create(destinationPath);
        stream.CopyTo(output);
    }

    private static void CopyInstallerToInstallDirectory()
    {
        var currentExe = Environment.ProcessPath
            ?? throw new InvalidOperationException("Kurulum EXE yolu okunamadi.");
        File.Copy(currentExe, InstallerPaths.InstalledSetupPath, overwrite: true);
    }

    private static void RegisterUninstaller()
    {
        using var key = Registry.LocalMachine.CreateSubKey(InstallerPaths.UninstallRegistryKey, writable: true)
            ?? throw new InvalidOperationException("Windows kaldirma kaydi olusturulamadi.");

        key.SetValue("DisplayName", "VideoWallPlayer", RegistryValueKind.String);
        key.SetValue("DisplayVersion", Application.ProductVersion, RegistryValueKind.String);
        key.SetValue("Publisher", "Nodera Software", RegistryValueKind.String);
        key.SetValue("InstallLocation", InstallerPaths.InstallDirectory, RegistryValueKind.String);
        key.SetValue("DisplayIcon", InstallerPaths.AppExePath, RegistryValueKind.String);
        key.SetValue("UninstallString", $"\"{InstallerPaths.InstalledSetupPath}\" /uninstall", RegistryValueKind.String);
        key.SetValue("QuietUninstallString", $"\"{InstallerPaths.InstalledSetupPath}\" /uninstall", RegistryValueKind.String);
        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
        key.SetValue("EstimatedSize", GetDirectorySizeKb(InstallerPaths.InstallDirectory), RegistryValueKind.DWord);
    }

    private static int GetDirectorySizeKb(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return 0;
        }

        var bytes = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
            .Sum(path => new FileInfo(path).Length);
        return (int)Math.Min(int.MaxValue, bytes / 1024);
    }

    private static void TryDeleteDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return;
        }

        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                NativeMethods.DeleteFileOnReboot(file);
            }
        }
    }

    private void StartInstalledApp()
    {
        if (File.Exists(InstallerPaths.AppExePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = InstallerPaths.AppExePath,
                UseShellExecute = true
            });
        }

        Close();
    }

    private void SetBusy(bool busy)
    {
        _primaryButton.Enabled = !busy;
        _cancelButton.Enabled = !busy;
        _desktopShortcutCheckBox.Enabled = !busy;
    }

    private void SetProgress(int value, string status)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetProgress(value, status));
            return;
        }

        _progressBar.Value = Math.Clamp(value, 0, 100);
        _statusLabel.Text = status;
    }
}

internal static class InstallerPaths
{
    public const string UninstallRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\VideoWallPlayer";

    public static string InstallDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "VideoWallPlayer");

    public static string AppExePath => Path.Combine(InstallDirectory, "VideoWallPlayer.exe");

    public static string InstalledSetupPath => Path.Combine(InstallDirectory, "VideoWallPlayerSetup.exe");

    public static bool IsRunningFromInstallDirectory()
    {
        var exePath = Environment.ProcessPath;
        return !string.IsNullOrWhiteSpace(exePath) &&
            exePath.StartsWith(InstallDirectory, StringComparison.OrdinalIgnoreCase);
    }
}

internal static class ShortcutService
{
    private static string DesktopShortcutPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "VideoWallPlayer.lnk");

    private static string StartMenuDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "VideoWallPlayer");

    private static string StartMenuShortcutPath =>
        Path.Combine(StartMenuDirectory, "VideoWallPlayer.lnk");

    public static void CreateDesktopShortcut() => CreateShortcut(DesktopShortcutPath);

    public static void CreateStartMenuShortcut()
    {
        Directory.CreateDirectory(StartMenuDirectory);
        CreateShortcut(StartMenuShortcutPath);
    }

    public static void DeleteDesktopShortcut() => TryDeleteFile(DesktopShortcutPath);

    public static void DeleteStartMenuShortcut()
    {
        TryDeleteFile(StartMenuShortcutPath);
        if (Directory.Exists(StartMenuDirectory) && !Directory.EnumerateFileSystemEntries(StartMenuDirectory).Any())
        {
            Directory.Delete(StartMenuDirectory);
        }
    }

    private static void CreateShortcut(string shortcutPath)
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Windows kisayol servisi bulunamadi.");
        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Windows kisayol servisi baslatilamadi.");
        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = InstallerPaths.AppExePath;
        shortcut.WorkingDirectory = InstallerPaths.InstallDirectory;
        shortcut.IconLocation = InstallerPaths.AppExePath + ",0";
        shortcut.Description = "VideoWallPlayer";
        shortcut.Save();
    }

    private static void TryDeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}

internal static partial class NativeMethods
{
    private const int MovefileDelayUntilReboot = 0x00000004;

    public static void DeleteFileOnReboot(string? path)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            MoveFileEx(path, null, MovefileDelayUntilReboot);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool MoveFileEx(string existingFileName, string? newFileName, int flags);
}
