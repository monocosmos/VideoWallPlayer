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

internal enum VideoPlacementMode
{
    Fit,
    FillCrop,
    Stretch,
    PixelPerfect,
    IntegerScale,
    ManualRectangle
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
    public VideoPlacementMode VideoPlacement { get; set; } = VideoPlacementMode.Fit;
    public int ManualVideoX { get; set; }
    public int ManualVideoY { get; set; }
    public int ManualVideoWidth { get; set; }
    public int ManualVideoHeight { get; set; }
    public GpuPreferenceMode GpuPreference { get; set; } = GpuPreferenceMode.WindowsDefault;
    public string? NamedGpu { get; set; }
    public int FileCachingMs { get; set; } = 3000;
    public string VideoDirectory { get; set; } = SettingsStore.DefaultVideoDirectory;
    public string Language { get; set; } = "tr";
}

internal static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string SettingsDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Nodera Software",
        "VideoWallPlayer");

    public static string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    public static string LegacySettingsPath => Path.Combine(AppContext.BaseDirectory, "settings.json");

    public static string DefaultVideoDirectory
    {
        get
        {
            var videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            if (string.IsNullOrWhiteSpace(videos))
            {
                videos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            return Path.Combine(videos, "VideoWallPlayer");
        }
    }

    public static AppSettings Load()
    {
        try
        {
            var settingsPath = File.Exists(SettingsPath)
                ? SettingsPath
                : File.Exists(LegacySettingsPath)
                    ? LegacySettingsPath
                    : null;

            if (settingsPath is null)
            {
                return new AppSettings();
            }

            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingsPath), JsonOptions) ?? new AppSettings();
            settings.Language = string.IsNullOrWhiteSpace(settings.Language) ? "tr" : settings.Language;
            settings.VideoDirectory = IsProgramFilesPath(settings.VideoDirectory) ? DefaultVideoDirectory : settings.VideoDirectory;
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        Directory.CreateDirectory(SettingsDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }

    private static bool IsProgramFilesPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return true;
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        return !string.IsNullOrWhiteSpace(programFiles) &&
            path.StartsWith(programFiles, StringComparison.OrdinalIgnoreCase);
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
    private const int StartupCachingMs = 350;
    private const int StandbyFirstFramePauseMs = 140;

    private readonly AppSettings _settings;
    private readonly LibVLC _libVlc;
    private readonly List<string> _sourcePlaylist;
    private readonly System.Windows.Forms.Timer _prepareNextTimer;
    private readonly System.Windows.Forms.Timer _freezeStandbyTimer;
    private readonly Random _random = new();

    private MediaPlayer _activeMediaPlayer;
    private MediaPlayer _standbyMediaPlayer;
    private VideoView _activeVideoView;
    private VideoView _standbyVideoView;
    private Media? _activeMedia;
    private Media? _standbyMedia;
    private int _standbyOrderIndex = -1;
    private bool _standbyPlaying;
    private bool _standbyPriming;
    private bool _standbyReady;
    private List<int> _playOrder = [];
    private int _orderIndex;
    private bool _isClosing;
    private bool _isPaused;
    private bool _playbackCursorHidden;

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

        _freezeStandbyTimer = new System.Windows.Forms.Timer
        {
            Interval = StandbyFirstFramePauseMs
        };

        _standbyVideoView = new VideoView
        {
            BackColor = Color.Black,
            MediaPlayer = _standbyMediaPlayer
        };

        _activeVideoView = new VideoView
        {
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
        HidePlaybackCursor();
        Activate();
        PlayCurrent();
        _prepareNextTimer.Start();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _isClosing = true;
        NativeMethods.AllowDisplaySleep();
        ShowLauncherCursor();
        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _prepareNextTimer.Dispose();
            _freezeStandbyTimer.Dispose();
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
        _activeMediaPlayer.Playing += MediaPlayerStarted;
        _standbyMediaPlayer.Playing += MediaPlayerStarted;
        _prepareNextTimer.Tick += (_, _) => PrepareNextWhenNeeded();
        _freezeStandbyTimer.Tick += (_, _) => FreezeStandbyOnFirstFrame();

        KeyDown += (_, e) =>
        {
            if (e.KeyCode is Keys.Escape or Keys.F11)
            {
                e.Handled = true;
                Close();
                return;
            }

            if (e.KeyCode == Keys.Space)
            {
                e.Handled = true;
                TogglePause();
                return;
            }

            if (_settings.Kiosk)
            {
                e.Handled = true;
            }
        };
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyVideoPlacement();
    }

    private MediaPlayer CreateMediaPlayer()
    {
        return new MediaPlayer(_libVlc)
        {
            EnableHardwareDecoding = _settings.HardwareAcceleration != HardwareAccelerationMode.Disabled,
            EnableKeyInput = false,
            EnableMouseInput = false,
            Mute = _settings.Muted,
            FileCaching = (uint)GetStartupCachingMs(_settings),
            NetworkCaching = (uint)GetStartupCachingMs(_settings)
        };
    }

    private static void ConfigurePlayerCaching(MediaPlayer player, int cachingMs)
    {
        player.FileCaching = (uint)cachingMs;
        player.NetworkCaching = (uint)cachingMs;
    }

    private void MediaPlayerEnded(object? sender, EventArgs e)
    {
        if (sender is MediaPlayer player && !ReferenceEquals(player, _activeMediaPlayer))
        {
            return;
        }

        RunOnUiThread(HandleVideoEnded);
    }

    private void MediaPlayerStarted(object? sender, EventArgs e)
    {
        if (sender is not MediaPlayer player)
        {
            return;
        }

        RunOnUiThread(() =>
        {
            ApplyVideoPlacement(player);
            if (ReferenceEquals(player, _standbyMediaPlayer) && _standbyPriming)
            {
                _freezeStandbyTimer.Stop();
                _freezeStandbyTimer.Start();
            }
        });
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
        ConfigurePlayerCaching(_activeMediaPlayer, GetStartupCachingMs(_settings));
        _activeMedia = CreateMedia(file, GetStartupCachingMs(_settings));
        _activeVideoView.BringToFront();
        _activeMediaPlayer.Play(_activeMedia);
        ApplyVideoPlacement(_activeMediaPlayer);
        _isPaused = false;
        PrepareNextWhenNeeded();
    }

    private void TogglePause()
    {
        if (_sourcePlaylist.Count == 0 || _activeMedia is null)
        {
            return;
        }

        _isPaused = !_isPaused;
        _activeMediaPlayer.SetPause(_isPaused);

        HidePlaybackCursor();
    }

    private void HidePlaybackCursor()
    {
        if (_playbackCursorHidden)
        {
            return;
        }

        _playbackCursorHidden = NativeMethods.HideCursorIfVisible();
    }

    private void ShowLauncherCursor()
    {
        NativeMethods.EnsureCursorVisible();
        _playbackCursorHidden = false;
    }

    private void PrepareNextWhenNeeded()
    {
        if (_isClosing || _activeMedia is null || _sourcePlaylist.Count == 0)
        {
            return;
        }

        var nextOrderIndex = GetNextOrderIndex();
        if (nextOrderIndex is null ||
            (_standbyPlaying && _standbyOrderIndex == nextOrderIndex))
        {
            return;
        }

        if (!_activeMediaPlayer.IsPlaying && !_isPaused)
        {
            return;
        }

        StartStandbyPreload(nextOrderIndex.Value);
    }

    private void StartStandbyPreload(int nextOrderIndex)
    {
        if (_isClosing || nextOrderIndex < 0 || nextOrderIndex >= _playOrder.Count)
        {
            return;
        }

        ClearStandby();

        var nextFile = _sourcePlaylist[_playOrder[nextOrderIndex]];
        var standbyCachingMs = GetStandbyCachingMs(_settings);
        ConfigurePlayerCaching(_standbyMediaPlayer, standbyCachingMs);
        _standbyMedia = CreateMedia(nextFile, standbyCachingMs);
        _standbyOrderIndex = nextOrderIndex;
        _standbyPriming = true;
        _standbyReady = false;
        _standbyMediaPlayer.Mute = true;
        _standbyVideoView.SendToBack();
        _standbyMediaPlayer.Play(_standbyMedia);
        ApplyVideoPlacement(_standbyMediaPlayer);
        _standbyPlaying = true;
        _activeVideoView.BringToFront();
    }

    private void FreezeStandbyOnFirstFrame()
    {
        _freezeStandbyTimer.Stop();

        if (_isClosing || !_standbyPriming || !_standbyPlaying || _standbyMedia is null)
        {
            return;
        }

        try
        {
            _standbyMediaPlayer.SetPause(true);
            if (_standbyMediaPlayer.Time > 0)
            {
                _standbyMediaPlayer.Time = 0;
            }
        }
        catch
        {
            // A few VLC output modules may reject seeks during startup; the player is still pre-opened.
        }

        _standbyPriming = false;
        _standbyReady = true;
        ApplyVideoPlacement(_standbyMediaPlayer);
    }

    private void ApplyVideoPlacement()
    {
        ApplyVideoPlacement(_activeMediaPlayer);
        ApplyVideoPlacement(_standbyMediaPlayer);
    }

    private void ApplyVideoPlacement(MediaPlayer player)
    {
        var view = ReferenceEquals(player, _activeMediaPlayer) ? _activeVideoView : _standbyVideoView;
        var bounds = CalculateVideoBounds(player);
        view.SuspendLayout();
        view.Dock = DockStyle.None;
        view.Bounds = bounds;
        ApplyPlayerAspect(player, bounds);
        view.ResumeLayout(false);
    }

    private Rectangle CalculateVideoBounds(MediaPlayer player)
    {
        var screen = ClientRectangle;
        if (screen.Width <= 0 || screen.Height <= 0)
        {
            return Rectangle.Empty;
        }

        if (_settings.VideoPlacement == VideoPlacementMode.ManualRectangle)
        {
            var manualWidth = _settings.ManualVideoWidth > 0 ? _settings.ManualVideoWidth : screen.Width;
            var manualHeight = _settings.ManualVideoHeight > 0 ? _settings.ManualVideoHeight : screen.Height;
            return new Rectangle(_settings.ManualVideoX, _settings.ManualVideoY, manualWidth, manualHeight);
        }

        var source = GetVideoSize(player);
        if (source.Width <= 0 || source.Height <= 0)
        {
            return screen;
        }

        return _settings.VideoPlacement switch
        {
            VideoPlacementMode.Stretch => screen,
            VideoPlacementMode.FillCrop => ScaleToCover(screen, source),
            VideoPlacementMode.PixelPerfect => Center(screen, source.Width, source.Height),
            VideoPlacementMode.IntegerScale => ScaleInteger(screen, source),
            _ => ScaleToFit(screen, source)
        };
    }

    private static Size GetVideoSize(MediaPlayer player)
    {
        try
        {
            uint width = 0;
            uint height = 0;
            return player.Size(0, ref width, ref height) && width > 0 && height > 0
                ? new Size((int)width, (int)height)
                : Size.Empty;
        }
        catch
        {
            return Size.Empty;
        }
    }

    private static Rectangle ScaleToFit(Rectangle screen, Size source)
    {
        var scale = Math.Min(screen.Width / (double)source.Width, screen.Height / (double)source.Height);
        return Center(screen, (int)Math.Round(source.Width * scale), (int)Math.Round(source.Height * scale));
    }

    private static Rectangle ScaleToCover(Rectangle screen, Size source)
    {
        var scale = Math.Max(screen.Width / (double)source.Width, screen.Height / (double)source.Height);
        return Center(screen, (int)Math.Round(source.Width * scale), (int)Math.Round(source.Height * scale));
    }

    private static Rectangle ScaleInteger(Rectangle screen, Size source)
    {
        var scale = Math.Max(1, Math.Min(screen.Width / source.Width, screen.Height / source.Height));
        return Center(screen, source.Width * scale, source.Height * scale);
    }

    private static Rectangle Center(Rectangle screen, int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        return new Rectangle(
            screen.Left + (screen.Width - width) / 2,
            screen.Top + (screen.Height - height) / 2,
            width,
            height);
    }

    private void ApplyPlayerAspect(MediaPlayer player, Rectangle bounds)
    {
        try
        {
            player.Scale = 0;
            player.AspectRatio = _settings.VideoPlacement == VideoPlacementMode.Stretch && bounds.Width > 0 && bounds.Height > 0
                ? $"{bounds.Width}:{bounds.Height}"
                : null;
        }
        catch
        {
            // Some output modules reject aspect updates while starting; the bounds still apply.
        }
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

    private Media CreateMedia(string file, int cachingMs)
    {
        var media = new Media(_libVlc, file, FromType.FromPath);
        media.AddOption($":file-caching={cachingMs}");
        media.AddOption($":network-caching={cachingMs}");
        media.AddOption($":avcodec-hw={GetHardwareOption(_settings.HardwareAcceleration)}");
        return media;
    }

    private void ClearStandby()
    {
        _freezeStandbyTimer.Stop();
        _standbyMediaPlayer.Stop();
        _standbyMedia?.Dispose();
        _standbyMedia = null;
        _standbyOrderIndex = -1;
        _standbyPlaying = false;
        _standbyPriming = false;
        _standbyReady = false;
    }

    private void HandleVideoEnded()
    {
        if (_isClosing || _sourcePlaylist.Count == 0)
        {
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
        if (SwapToStandbyIfAvailable())
        {
            return;
        }

        PlayCurrent();
    }

    private bool SwapToStandbyIfAvailable()
    {
        if (!_standbyPlaying || _standbyMedia is null || _standbyOrderIndex != _orderIndex)
        {
            return false;
        }

        _freezeStandbyTimer.Stop();
        var wasStandbyReady = _standbyReady;
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
        _standbyPriming = false;
        _standbyReady = false;

        _activeVideoView.BringToFront();
        _activeMediaPlayer.Mute = _settings.Muted;
        if (wasStandbyReady)
        {
            try
            {
                _activeMediaPlayer.Time = 0;
            }
            catch
            {
                // If the decoder refuses a zero seek, continuing is still faster than reopening.
            }
        }

        _activeMediaPlayer.SetPause(false);
        previousPlayer.Stop();
        previousMedia?.Dispose();
        PrepareNextWhenNeeded();
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
        var startupCachingMs = GetStartupCachingMs(settings);
        var options = new List<string>
        {
            "--no-video-title-show",
            "--no-osd",
            "--no-stats",
            "--quiet",
            $"--file-caching={startupCachingMs}",
            $"--network-caching={startupCachingMs}",
            $"--avcodec-hw={GetHardwareOption(settings.HardwareAcceleration)}"
        };

        var videoOutput = GetVideoOutputOption(settings.VideoOutput);
        if (videoOutput is not null)
        {
            options.Add($"--vout={videoOutput}");
        }

        return [.. options];
    }

    private static int GetStartupCachingMs(AppSettings settings)
    {
        return Math.Clamp(Math.Min(settings.FileCachingMs, StartupCachingMs), 100, StartupCachingMs);
    }

    private static int GetStandbyCachingMs(AppSettings settings)
    {
        return Math.Clamp(settings.FileCachingMs, 100, 30000);
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
    private const int CursorShowing = 0x00000001;
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaUseImmersiveDarkModeLegacy = 19;
    private const int DwmwaBorderColor = 34;
    private const int DwmwaCaptionColor = 35;
    private const int DwmwaTextColor = 36;

    [Flags]
    private enum ExecutionState : uint
    {
        Continuous = 0x80000000,
        SystemRequired = 0x00000001,
        DisplayRequired = 0x00000002
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CursorInfo
    {
        public int Size;
        public int Flags;
        public IntPtr Cursor;
        public NativePoint ScreenPosition;
    }

    public static void PreventDisplaySleep()
    {
        SetThreadExecutionState(ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.DisplayRequired);
    }

    public static void AllowDisplaySleep()
    {
        SetThreadExecutionState(ExecutionState.Continuous);
    }

    public static void ApplyDarkWindowFrame(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var enabled = 1;
        _ = DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode, ref enabled, sizeof(int));
        _ = DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkModeLegacy, ref enabled, sizeof(int));

        var frameColor = 0x00150F0C;
        var textColor = 0x00F1E5DD;
        _ = DwmSetWindowAttribute(handle, DwmwaCaptionColor, ref frameColor, sizeof(int));
        _ = DwmSetWindowAttribute(handle, DwmwaBorderColor, ref frameColor, sizeof(int));
        _ = DwmSetWindowAttribute(handle, DwmwaTextColor, ref textColor, sizeof(int));
    }

    public static bool HideCursorIfVisible()
    {
        if (!IsCursorVisible())
        {
            return false;
        }

        ShowCursor(false);
        return true;
    }

    public static void EnsureCursorVisible()
    {
        for (var i = 0; i < 10 && !IsCursorVisible(); i++)
        {
            ShowCursor(true);
        }
    }

    private static bool IsCursorVisible()
    {
        var info = new CursorInfo
        {
            Size = Marshal.SizeOf<CursorInfo>()
        };

        return GetCursorInfo(ref info) && (info.Flags & CursorShowing) == CursorShowing;
    }

    [DllImport("kernel32.dll")]
    private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);

    [DllImport("user32.dll")]
    private static extern bool GetCursorInfo(ref CursorInfo cursorInfo);

    [DllImport("user32.dll")]
    private static extern int ShowCursor(bool show);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
}
