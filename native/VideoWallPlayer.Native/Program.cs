using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using Microsoft.Win32;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace VideoWallPlayer.Native;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Core.Initialize(ResolveLibVlcDirectory());
        ApplicationConfiguration.Initialize();
        Application.Run(new LauncherForm());
    }

    private static string? ResolveLibVlcDirectory()
    {
        var architectureDirectory = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "win-x64",
            Architecture.X86 => "win-x86",
            Architecture.Arm64 => "win-arm64",
            _ => null
        };

        if (architectureDirectory is null)
        {
            return null;
        }

        var libVlcDirectory = Path.Combine(AppContext.BaseDirectory, "libvlc", architectureDirectory);
        return Directory.Exists(libVlcDirectory) ? libVlcDirectory : null;
    }
}

internal enum RepeatMode
{
    None,
    All,
    One
}

internal enum HardwareAccelerationMode
{
    Auto,
    Disabled,
    D3D11VA,
    DXVA2
}

internal enum VideoOutputMode
{
    Auto,
    Direct3D11,
    Direct3D9,
    OpenGL
}

internal enum GpuPreferenceMode
{
    WindowsDefault,
    PowerSaving,
    HighPerformance,
    NamedGpu
}

internal sealed class AppSettings
{
    public List<string> Playlist { get; set; } = [];
    public RepeatMode RepeatMode { get; set; } = RepeatMode.All;
    public bool Shuffle { get; set; }
    public bool Fullscreen { get; set; } = true;
    public bool Kiosk { get; set; }
    public bool Muted { get; set; }
    public int DisplayIndex { get; set; }
    public HardwareAccelerationMode HardwareAcceleration { get; set; } = HardwareAccelerationMode.Auto;
    public VideoOutputMode VideoOutput { get; set; } = VideoOutputMode.Auto;
    public GpuPreferenceMode GpuPreference { get; set; } = GpuPreferenceMode.WindowsDefault;
    public string? NamedGpu { get; set; }
    public int FileCachingMs { get; set; } = 3000;
    public string VideoDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "videos");
    public string Language { get; set; } = "tr";
}

internal static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new AppSettings();
            }

            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath), JsonOptions) ?? new AppSettings();
            settings.Language = string.IsNullOrWhiteSpace(settings.Language) ? "tr" : settings.Language;
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(AppContext.BaseDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }
}

internal static class SupportedVideo
{
    public static readonly string[] Extensions =
    [
        ".3g2", ".3gp", ".asf", ".avi", ".divx", ".f4v", ".flv", ".h264", ".h265", ".hevc",
        ".m2t", ".m2ts", ".m2v", ".m4v", ".mkv", ".mov", ".mp4", ".mpeg", ".mpg", ".mts",
        ".mxf", ".ogm", ".ogv", ".rm", ".rmvb", ".ts", ".vob", ".webm", ".wmv"
    ];

    public static string FileDialogFilter(Localizer localizer)
    {
        return localizer.T("video.files") + "|" + string.Join(";", Extensions.Select(extension => $"*{extension}")) +
            "|" + localizer.T("all.files") + "|*.*";
    }

    public static bool IsSupported(string filePath)
    {
        return Extensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase);
    }
}

internal sealed record GpuInfo(string Name, string? DriverVersion, string? PnpDeviceId);

internal static class GpuService
{
    private const string UserGpuPreferencesKey = @"Software\Microsoft\DirectX\UserGpuPreferences";

    public static List<GpuInfo> GetDetectedGpus()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("select Name, DriverVersion, PNPDeviceID from Win32_VideoController");
            return searcher
                .Get()
                .Cast<ManagementObject>()
                .Select(item => new GpuInfo(
                    Convert.ToString(item["Name"]) ?? "Bilinmeyen GPU",
                    Convert.ToString(item["DriverVersion"]),
                    Convert.ToString(item["PNPDeviceID"])))
                .Where(gpu => !string.IsNullOrWhiteSpace(gpu.Name))
                .Where(gpu => !IsVirtualDisplayAdapter(gpu.Name))
                .DistinctBy(gpu => NormalizeGpuName(gpu.Name))
                .OrderBy(gpu => gpu.Name)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public static string ApplyWindowsGpuPreference(AppSettings settings, Localizer localizer)
    {
        var preference = ResolveWindowsPreference(settings);
        var executablePath = Application.ExecutablePath;

        using var key = Registry.CurrentUser.CreateSubKey(UserGpuPreferencesKey, true);

        if (preference == 0)
        {
            key?.DeleteValue(executablePath, false);
            return localizer.T("gpu.message.default");
        }

        key?.SetValue(executablePath, $"GpuPreference={preference};", RegistryValueKind.String);

        return preference == 2
            ? localizer.T("gpu.message.high")
            : localizer.T("gpu.message.power");
    }

    private static int ResolveWindowsPreference(AppSettings settings)
    {
        return settings.GpuPreference switch
        {
            GpuPreferenceMode.PowerSaving => 1,
            GpuPreferenceMode.HighPerformance => 2,
            _ => 0
        };
    }

    private static bool IsVirtualDisplayAdapter(string name)
    {
        var normalized = name.ToLowerInvariant();
        return normalized.Contains("microsoft basic") ||
            normalized.Contains("remote") ||
            normalized.Contains("mirror") ||
            normalized.Contains("virtual") ||
            normalized.Contains("indirect");
    }

    private static string NormalizeGpuName(string name)
    {
        return name.Trim().ToLowerInvariant();
    }
}

internal sealed class ComboItem<T>
{
    public ComboItem(string text, T value)
    {
        Text = text;
        Value = value;
    }

    public string Text { get; }
    public T Value { get; }

    public override string ToString() => Text;
}

internal sealed class VideoWallForm : Form
{
    private const int TransitionOverlapMs = 650;

    private readonly AppSettings _settings;
    private readonly LibVLC _libVlc;
    private readonly List<string> _sourcePlaylist;
    private readonly System.Windows.Forms.Timer _prepareNextTimer;
    private readonly Random _random = new();

    private MediaPlayer _activeMediaPlayer;
    private MediaPlayer _standbyMediaPlayer;
    private VideoView _activeVideoView;
    private VideoView _standbyVideoView;
    private Media? _activeMedia;
    private Media? _standbyMedia;
    private int _standbyOrderIndex = -1;
    private bool _standbyPlaying;
    private List<int> _playOrder = [];
    private int _orderIndex;
    private bool _isClosing;

    public VideoWallForm(AppSettings settings)
    {
        _settings = settings;
        _sourcePlaylist = LoadPlaylist(settings);
        _playOrder = BuildPlayOrder();

        _libVlc = new LibVLC(BuildVlcOptions(settings));
        _activeMediaPlayer = CreateMediaPlayer();
        _standbyMediaPlayer = CreateMediaPlayer();

        _prepareNextTimer = new System.Windows.Forms.Timer
        {
            Interval = 200
        };

        _standbyVideoView = new VideoView
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
            MediaPlayer = _standbyMediaPlayer
        };

        _activeVideoView = new VideoView
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
            MediaPlayer = _activeMediaPlayer
        };

        SuspendLayout();
        BackColor = Color.Black;
        KeyPreview = true;
        ShowIcon = false;
        Text = string.Empty;
        Controls.Add(_standbyVideoView);
        Controls.Add(_activeVideoView);
        ApplyWindowMode();
        ResumeLayout(false);

        ApplyScreenBounds();
        WireEvents();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        NativeMethods.PreventDisplaySleep();
        Cursor.Hide();
        Activate();
        PlayCurrent();
        _prepareNextTimer.Start();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _isClosing = true;
        NativeMethods.AllowDisplaySleep();
        Cursor.Show();
        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _prepareNextTimer.Dispose();
            _activeMedia?.Dispose();
            _standbyMedia?.Dispose();
            _activeMediaPlayer.Dispose();
            _standbyMediaPlayer.Dispose();
            _libVlc.Dispose();
        }

        base.Dispose(disposing);
    }

    private void ApplyWindowMode()
    {
        ControlBox = false;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
    }

    private void WireEvents()
    {
        _activeMediaPlayer.EndReached += MediaPlayerEnded;
        _activeMediaPlayer.EncounteredError += MediaPlayerEnded;
        _standbyMediaPlayer.EndReached += MediaPlayerEnded;
        _standbyMediaPlayer.EncounteredError += MediaPlayerEnded;
        _prepareNextTimer.Tick += (_, _) => PrepareNextWhenCloseToEnd();

        KeyDown += (_, e) =>
        {
            if (e.KeyCode is Keys.Escape or Keys.F11)
            {
                e.Handled = true;
                Close();
                return;
            }

            if (_settings.Kiosk)
            {
                e.Handled = true;
            }
        };
    }

    private MediaPlayer CreateMediaPlayer()
    {
        return new MediaPlayer(_libVlc)
        {
            EnableHardwareDecoding = _settings.HardwareAcceleration != HardwareAccelerationMode.Disabled,
            EnableKeyInput = false,
            EnableMouseInput = false,
            Mute = _settings.Muted,
            FileCaching = (uint)_settings.FileCachingMs,
            NetworkCaching = (uint)_settings.FileCachingMs
        };
    }

    private void MediaPlayerEnded(object? sender, EventArgs e)
    {
        if (sender is MediaPlayer player && !ReferenceEquals(player, _activeMediaPlayer))
        {
            return;
        }

        RunOnUiThread(HandleVideoEnded);
    }

    private void PlayCurrent()
    {
        if (_isClosing || _sourcePlaylist.Count == 0 || _playOrder.Count == 0)
        {
            return;
        }

        var file = _sourcePlaylist[_playOrder[_orderIndex]];
        _activeMediaPlayer.Stop();
        _activeMedia?.Dispose();
        _activeMedia = CreateMedia(file);
        _activeVideoView.BringToFront();
        _activeMediaPlayer.Play(_activeMedia);
    }

    private void PrepareNextWhenCloseToEnd()
    {
        if (_isClosing || !_activeMediaPlayer.IsPlaying || _sourcePlaylist.Count < 2)
        {
            return;
        }

        var remaining = _activeMediaPlayer.Length - _activeMediaPlayer.Time;

        if (_activeMediaPlayer.Length > 0 && remaining <= TransitionOverlapMs)
        {
            StartStandbyPlayback();
        }
    }

    private void StartStandbyPlayback()
    {
        var nextOrderIndex = GetNextOrderIndex();
        if (nextOrderIndex is null ||
            nextOrderIndex == _orderIndex ||
            (_standbyPlaying && _standbyOrderIndex == nextOrderIndex))
        {
            return;
        }

        ClearStandby();

        var nextFile = _sourcePlaylist[_playOrder[nextOrderIndex.Value]];
        _standbyMedia = CreateMedia(nextFile);
        _standbyOrderIndex = nextOrderIndex.Value;
        _standbyVideoView.SendToBack();
        _standbyMediaPlayer.Play(_standbyMedia);
        _standbyPlaying = true;
        _activeVideoView.BringToFront();
    }

    private int? GetNextOrderIndex()
    {
        if (_sourcePlaylist.Count == 0 || _playOrder.Count == 0)
        {
            return null;
        }

        if (_settings.RepeatMode == RepeatMode.One)
        {
            return _orderIndex;
        }

        var next = _orderIndex + 1;

        if (next < _playOrder.Count)
        {
            return next;
        }

        return _settings.RepeatMode == RepeatMode.All ? 0 : null;
    }

    private Media CreateMedia(string file)
    {
        var media = new Media(_libVlc, file, FromType.FromPath);
        media.AddOption($":file-caching={_settings.FileCachingMs}");
        media.AddOption($":network-caching={_settings.FileCachingMs}");
        media.AddOption($":avcodec-hw={GetHardwareOption(_settings.HardwareAcceleration)}");
        return media;
    }

    private void ClearStandby()
    {
        _standbyMediaPlayer.Stop();
        _standbyMedia?.Dispose();
        _standbyMedia = null;
        _standbyOrderIndex = -1;
        _standbyPlaying = false;
    }

    private void HandleVideoEnded()
    {
        if (_isClosing || _sourcePlaylist.Count == 0)
        {
            return;
        }

        if (_settings.RepeatMode == RepeatMode.One)
        {
            PlayCurrent();
            return;
        }

        var nextOrderIndex = GetNextOrderIndex();
        if (nextOrderIndex is null)
        {
            ClearStandby();
            Close();
            return;
        }

        _orderIndex = nextOrderIndex.Value;
        if (SwapToStandbyIfReady())
        {
            return;
        }

        PlayCurrent();
    }

    private bool SwapToStandbyIfReady()
    {
        if (!_standbyPlaying || _standbyMedia is null || _standbyOrderIndex != _orderIndex)
        {
            return false;
        }

        var previousPlayer = _activeMediaPlayer;
        var previousView = _activeVideoView;
        var previousMedia = _activeMedia;

        _activeMediaPlayer = _standbyMediaPlayer;
        _activeVideoView = _standbyVideoView;
        _activeMedia = _standbyMedia;

        _standbyMediaPlayer = previousPlayer;
        _standbyVideoView = previousView;
        _standbyMedia = null;
        _standbyOrderIndex = -1;
        _standbyPlaying = false;

        _activeVideoView.BringToFront();
        previousPlayer.Stop();
        previousMedia?.Dispose();
        return true;
    }

    private List<int> BuildPlayOrder()
    {
        var order = Enumerable.Range(0, _sourcePlaylist.Count).ToList();

        if (!_settings.Shuffle || order.Count < 2)
        {
            return order;
        }

        for (var i = order.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        return order;
    }

    private void RunOnUiThread(Action action)
    {
        if (_isClosing || IsDisposed)
        {
            return;
        }

        try
        {
            BeginInvoke(action);
        }
        catch (InvalidOperationException)
        {
            // VLC can raise events while the window is closing.
        }
    }

    private void ApplyScreenBounds()
    {
        if (!_settings.Fullscreen && !_settings.Kiosk)
        {
            return;
        }

        var screens = Screen.AllScreens;
        var selectedScreen = screens[Math.Clamp(_settings.DisplayIndex, 0, screens.Length - 1)];
        Bounds = selectedScreen.Bounds;
    }

    private static List<string> LoadPlaylist(AppSettings settings)
    {
        var selectedFiles = settings.Playlist
            .Where(File.Exists)
            .Where(SupportedVideo.IsSupported)
            .ToList();

        if (selectedFiles.Count > 0)
        {
            return selectedFiles;
        }

        Directory.CreateDirectory(settings.VideoDirectory);
        return Directory
            .EnumerateFiles(settings.VideoDirectory, "*.*", SearchOption.TopDirectoryOnly)
            .Where(SupportedVideo.IsSupported)
            .OrderBy(Path.GetFileName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static string[] BuildVlcOptions(AppSettings settings)
    {
        var options = new List<string>
        {
            "--no-video-title-show",
            "--no-osd",
            "--no-stats",
            "--quiet",
            $"--file-caching={settings.FileCachingMs}",
            $"--network-caching={settings.FileCachingMs}",
            $"--avcodec-hw={GetHardwareOption(settings.HardwareAcceleration)}"
        };

        var videoOutput = GetVideoOutputOption(settings.VideoOutput);
        if (videoOutput is not null)
        {
            options.Add($"--vout={videoOutput}");
        }

        return [.. options];
    }

    private static string GetHardwareOption(HardwareAccelerationMode mode)
    {
        return mode switch
        {
            HardwareAccelerationMode.Disabled => "none",
            HardwareAccelerationMode.D3D11VA => "d3d11va",
            HardwareAccelerationMode.DXVA2 => "dxva2",
            _ => "any"
        };
    }

    private static string? GetVideoOutputOption(VideoOutputMode mode)
    {
        return mode switch
        {
            VideoOutputMode.Direct3D11 => "direct3d11",
            VideoOutputMode.Direct3D9 => "direct3d9",
            VideoOutputMode.OpenGL => "glwin32",
            _ => null
        };
    }
}

internal static class NativeMethods
{
    [Flags]
    private enum ExecutionState : uint
    {
        Continuous = 0x80000000,
        SystemRequired = 0x00000001,
        DisplayRequired = 0x00000002
    }

    public static void PreventDisplaySleep()
    {
        SetThreadExecutionState(ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.DisplayRequired);
    }

    public static void AllowDisplaySleep()
    {
        SetThreadExecutionState(ExecutionState.Continuous);
    }

    [DllImport("kernel32.dll")]
    private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);
}
