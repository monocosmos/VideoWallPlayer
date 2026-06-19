namespace VideoWallPlayer.Native;

public sealed partial class LauncherForm : Form
{
    private const int ResizeGripSize = 8;
    private const int TitleBarHeight = 38;
    private const int WmNclButtonDown = 0x00A1;
    private const int WmNcHitTest = 0x0084;
    private const int HtCaption = 0x02;
    private const int HtLeft = 10;
    private const int HtRight = 11;
    private const int HtTop = 12;
    private const int HtTopLeft = 13;
    private const int HtTopRight = 14;
    private const int HtBottom = 15;
    private const int HtBottomLeft = 16;
    private const int HtBottomRight = 17;

    private static readonly Color BackgroundColor = Color.FromArgb(17, 20, 28);
    private static readonly Color SurfaceColor = Color.FromArgb(25, 30, 40);
    private static readonly Color FieldColor = Color.FromArgb(12, 15, 22);
    private static readonly Color BorderColor = Color.FromArgb(58, 70, 88);
    private static readonly Color TextMutedColor = Color.FromArgb(172, 187, 205);
    private static readonly Color AccentColor = Color.FromArgb(0, 202, 218);
    private static readonly Color AccentHoverColor = Color.FromArgb(16, 226, 238);

    private readonly AppSettings _settings;
    private readonly Localizer _localizer;

    private Panel? _titleBarPanel;
    private Label? _titleBarLabel;
    private Button? _minimizeButton;
    private Button? _maximizeButton;
    private Button? _closeButton;

    private ListBox _playlistList => playlistListBox;
    private ComboBox _languageCombo => languageComboBox;
    private ComboBox _repeatCombo => repeatComboBox;
    private ComboBox _displayCombo => displayComboBox;
    private ComboBox _hardwareCombo => hardwareComboBox;
    private ComboBox _videoOutputCombo => videoOutputComboBox;
    private ComboBox _gpuCombo => gpuComboBox;
    private Label _gpuInfoLabel => gpuInfoLabel;
    private CheckBox _shuffleCheck => shuffleCheckBox;
    private CheckBox _fullscreenCheck => fullscreenCheckBox;
    private CheckBox _kioskCheck => kioskCheckBox;
    private CheckBox _mutedCheck => mutedCheckBox;
    private NumericUpDown _cacheInput => cacheNumericUpDown;
    private Label _statusLabel => statusLabel;

    public LauncherForm()
    {
        InitializeComponent();

        _settings = SettingsStore.Load();
        _localizer = new Localizer(_settings.Language);

        Text = _localizer.T("app.title");
        RightToLeft = _localizer.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = _localizer.IsRightToLeft;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Icon;

        ApplyRuntimeStyle();
        ApplyLocalizedText();
        LoadBrandImages();
        PopulateStaticChoices();
        LoadSettingsIntoForm();
    }

    private void ApplyRuntimeStyle()
    {
        Font = new Font("Segoe UI", 11F);
        MinimumSize = new Size(960, 620);
        FormBorderStyle = FormBorderStyle.None;
        DoubleBuffered = true;
        BackColor = BackgroundColor;
        rootTableLayoutPanel.BackColor = BackgroundColor;
        rootTableLayoutPanel.Padding = new Padding(24, 20, 24, 24);
        playlistButtonsFlowLayoutPanel.WrapContents = true;
        playlistButtonsFlowLayoutPanel.AutoSize = true;
        playlistButtonsFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        actionsFlowLayoutPanel.WrapContents = true;
        actionsFlowLayoutPanel.AutoSize = true;
        actionsFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        playbackOptionsFlowLayoutPanel.WrapContents = true;
        playbackOptionsFlowLayoutPanel.AutoSize = true;
        playbackOptionsFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        settingsTableLayoutPanel.AutoScroll = true;
        settingsTableLayoutPanel.BackColor = SurfaceColor;
        settingsTableLayoutPanel.Padding = new Padding(18, 14, 18, 14);
        playlistTableLayoutPanel.BackColor = SurfaceColor;
        playlistTableLayoutPanel.Padding = new Padding(18, 14, 18, 14);
        brandTableLayoutPanel.BackColor = BackgroundColor;
        titleTableLayoutPanel.BackColor = BackgroundColor;

        logoPictureBox.BackColor = BackgroundColor;
        modelPictureBox.BackColor = BackgroundColor;
        modelPictureBox.SizeMode = PictureBoxSizeMode.Zoom;

        titleLabel.Font = new Font("Segoe UI Semibold", 28F);
        titleLabel.ForeColor = Color.White;
        playlistHeaderLabel.Font = new Font("Segoe UI Semibold", 18F);
        playlistHeaderLabel.ForeColor = Color.White;
        settingsHeaderLabel.Font = new Font("Segoe UI Semibold", 18F);
        settingsHeaderLabel.ForeColor = Color.White;
        subtitleLabel.ForeColor = Color.FromArgb(161, 237, 240);
        statusLabel.ForeColor = TextMutedColor;
        gpuInfoLabel.ForeColor = TextMutedColor;
        gpuNoteLabel.ForeColor = TextMutedColor;

        playlistListBox.Font = new Font("Segoe UI", 11F);
        playlistListBox.BackColor = FieldColor;
        playlistListBox.ForeColor = Color.White;
        playlistListBox.BorderStyle = BorderStyle.FixedSingle;
        playlistListBox.ItemHeight = 22;

        foreach (var label in new[]
        {
            languageLabel, repeatLabel, displayLabel, hardwareLabel, outputLabel, gpuLabel, cacheLabel
        })
        {
            label.AutoSize = true;
            label.Dock = DockStyle.Top;
            label.ForeColor = TextMutedColor;
            label.Margin = new Padding(0, 8, 0, 0);
        }

        foreach (var comboBox in new[]
        {
            languageComboBox, repeatComboBox, displayComboBox, hardwareComboBox, videoOutputComboBox, gpuComboBox
        })
        {
            comboBox.Dock = DockStyle.Top;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.BackColor = FieldColor;
            comboBox.ForeColor = Color.White;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.Margin = new Padding(0, 4, 0, 8);
            comboBox.Height = 34;
        }

        cacheNumericUpDown.BackColor = FieldColor;
        cacheNumericUpDown.ForeColor = Color.White;
        cacheNumericUpDown.BorderStyle = BorderStyle.FixedSingle;

        foreach (var checkBox in new[] { shuffleCheckBox, fullscreenCheckBox, kioskCheckBox, mutedCheckBox })
        {
            checkBox.AutoSize = true;
            checkBox.ForeColor = Color.White;
            checkBox.Margin = new Padding(0, 0, 18, 0);
        }

        foreach (var button in new[]
        {
            addVideosButton, addFolderButton, removeSelectedButton, clearButton, moveUpButton,
            moveDownButton, saveSettingsButton, exitButton
        })
        {
            StyleButton(button, Color.FromArgb(38, 48, 64), Color.FromArgb(48, 62, 82), BorderColor, 92, 32);
        }

        StyleButton(playButton, AccentColor, AccentHoverColor, Color.FromArgb(91, 246, 247), 150, 36);
        playButton.FlatStyle = FlatStyle.Flat;
        playButton.Font = new Font("Segoe UI Semibold", 10.5F);
        playButton.ForeColor = Color.FromArgb(4, 19, 27);
        playButton.Margin = new Padding(0, 0, 8, 8);
        playButton.UseVisualStyleBackColor = false;
        playButton.Width = 150;

        InstallCustomTitleBar();
        Resize += (_, _) => ApplyResponsiveStyle();
        ApplyResponsiveStyle();
    }

    private static void StyleButton(Button button, Color backColor, Color hoverColor, Color borderColor, int minWidth, int height)
    {
        button.AutoSize = true;
        button.BackColor = backColor;
        button.Cursor = Cursors.Hand;
        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.MouseDownBackColor = hoverColor;
        button.FlatAppearance.MouseOverBackColor = hoverColor;
        button.FlatStyle = FlatStyle.Flat;
        button.Font = new Font("Segoe UI Semibold", 9.5F);
        button.ForeColor = Color.White;
        button.Height = height;
        button.Margin = new Padding(0, 0, 8, 7);
        button.MinimumSize = new Size(minWidth, height);
        button.Padding = new Padding(10, 0, 10, 0);
        button.UseVisualStyleBackColor = false;
        button.Resize += (_, _) => ApplyRoundedRegion(button);
        ApplyRoundedRegion(button);
    }

    private static void ApplyRoundedRegion(Control control)
    {
        if (control.Width <= 0 || control.Height <= 0)
        {
            return;
        }

        var bounds = new Rectangle(0, 0, control.Width, control.Height);
        using var path = CreateRoundedRectangle(bounds, 8);
        control.Region = new Region(path);
    }

    private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var diameter = radius * 2;
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void ApplyResponsiveStyle()
    {
        var compact = ClientSize.Width < 1080;
        rootTableLayoutPanel.Padding = compact ? new Padding(16, 14, 16, 16) : new Padding(24, 20, 24, 24);
        rootTableLayoutPanel.RowStyles[0].Height = compact ? 112F : 132F;
        brandTableLayoutPanel.ColumnStyles[0].Width = compact ? 88F : 118F;
        brandTableLayoutPanel.ColumnStyles[2].Width = compact ? 96F : 126F;
        titleLabel.Font = new Font("Segoe UI Semibold", compact ? 22F : 26F);
        subtitleLabel.Font = new Font("Segoe UI", compact ? 10F : 11F);
    }

    private void InstallCustomTitleBar()
    {
        if (_titleBarPanel is not null)
        {
            return;
        }

        _titleBarPanel = new Panel
        {
            BackColor = Color.FromArgb(12, 15, 21),
            Dock = DockStyle.Top,
            Height = TitleBarHeight,
            Padding = new Padding(14, 0, 8, 0)
        };

        var iconLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Left,
            Font = new Font("Segoe UI Symbol", 11F),
            ForeColor = AccentColor,
            Text = "■",
            TextAlign = ContentAlignment.MiddleLeft,
            Width = 18
        };

        _titleBarLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 9.5F),
            ForeColor = Color.FromArgb(221, 229, 241),
            Text = Text,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _closeButton = CreateChromeButton("x", (_, _) => Close(), true);
        _maximizeButton = CreateChromeButton("□", (_, _) => ToggleWindowState());
        _minimizeButton = CreateChromeButton("−", (_, _) => WindowState = FormWindowState.Minimized);

        _titleBarPanel.Controls.Add(_titleBarLabel);
        _titleBarPanel.Controls.Add(iconLabel);
        _titleBarPanel.Controls.Add(_closeButton);
        _titleBarPanel.Controls.Add(_maximizeButton);
        _titleBarPanel.Controls.Add(_minimizeButton);
        _titleBarPanel.MouseDown += TitleBar_MouseDown;
        _titleBarPanel.DoubleClick += (_, _) => ToggleWindowState();
        _titleBarLabel.MouseDown += TitleBar_MouseDown;
        _titleBarLabel.DoubleClick += (_, _) => ToggleWindowState();
        iconLabel.MouseDown += TitleBar_MouseDown;

        Controls.Remove(rootTableLayoutPanel);
        Controls.Add(rootTableLayoutPanel);
        Controls.Add(_titleBarPanel);
        _titleBarPanel.BringToFront();
    }

    private static Button CreateChromeButton(string text, EventHandler clickHandler, bool closeButton = false)
    {
        var button = new Button
        {
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Dock = DockStyle.Right,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(218, 226, 238),
            Height = TitleBarHeight,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Text = text,
            UseVisualStyleBackColor = false,
            Width = 44
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = closeButton ? Color.FromArgb(206, 54, 74) : Color.FromArgb(31, 39, 52);
        button.FlatAppearance.MouseDownBackColor = closeButton ? Color.FromArgb(174, 43, 60) : Color.FromArgb(41, 51, 68);
        button.Click += clickHandler;
        return button;
    }

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        NativeMethods.ReleaseCapture();
        NativeMethods.SendMessage(Handle, WmNclButtonDown, HtCaption, 0);
    }

    private void ToggleWindowState()
    {
        WindowState = WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal
            : FormWindowState.Maximized;
    }

    protected override void WndProc(ref Message message)
    {
        base.WndProc(ref message);

        if (message.Msg != WmNcHitTest || WindowState == FormWindowState.Maximized)
        {
            return;
        }

        var cursor = PointToClient(new Point(message.LParam.ToInt32()));
        var left = cursor.X <= ResizeGripSize;
        var right = cursor.X >= ClientSize.Width - ResizeGripSize;
        var top = cursor.Y <= ResizeGripSize;
        var bottom = cursor.Y >= ClientSize.Height - ResizeGripSize;

        message.Result = (IntPtr)((left, right, top, bottom) switch
        {
            (true, false, true, false) => HtTopLeft,
            (false, true, true, false) => HtTopRight,
            (true, false, false, true) => HtBottomLeft,
            (false, true, false, true) => HtBottomRight,
            (true, false, false, false) => HtLeft,
            (false, true, false, false) => HtRight,
            (false, false, true, false) => HtTop,
            (false, false, false, true) => HtBottom,
            _ => message.Result.ToInt32()
        });
    }

    private void ApplyLocalizedText()
    {
        Text = _localizer.T("app.title");
        if (_titleBarLabel is not null)
        {
            _titleBarLabel.Text = Text;
        }

        titleLabel.Text = _localizer.T("app.title");
        subtitleLabel.Text = _localizer.T("brand.subtitle");
        playlistHeaderLabel.Text = _localizer.T("playlist");
        settingsHeaderLabel.Text = _localizer.T("settings");
        languageLabel.Text = _localizer.T("language");
        repeatLabel.Text = _localizer.T("repeat.mode");
        displayLabel.Text = _localizer.T("target.display");
        hardwareLabel.Text = _localizer.T("hardware.acceleration");
        outputLabel.Text = _localizer.T("video.output");
        gpuLabel.Text = _localizer.T("gpu.preference");
        cacheLabel.Text = _localizer.T("cache");
        gpuNoteLabel.Text = _localizer.T("gpu.note");

        addVideosButton.Text = _localizer.T("add.videos");
        addFolderButton.Text = _localizer.T("add.folder");
        removeSelectedButton.Text = _localizer.T("remove.selected");
        clearButton.Text = _localizer.T("clear");
        moveUpButton.Text = _localizer.T("move.up");
        moveDownButton.Text = _localizer.T("move.down");
        playButton.Text = _localizer.T("play");
        saveSettingsButton.Text = _localizer.T("save.settings");
        exitButton.Text = _localizer.T("exit");

        _shuffleCheck.Text = _localizer.T("shuffle");
        _fullscreenCheck.Text = _localizer.T("fullscreen");
        _kioskCheck.Text = _localizer.T("kiosk");
        _mutedCheck.Text = _localizer.T("muted");
        _fullscreenCheck.Checked = true;
        _fullscreenCheck.Enabled = false;
        _gpuInfoLabel.Text = BuildGpuInfoText();
    }

    private void LoadBrandImages()
    {
        logoPictureBox.Image = LoadAssetImage("brand-logo.png");
        modelPictureBox.Image = LoadAssetImage("brand-model.png");
    }

    private static Image? LoadAssetImage(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
        return File.Exists(path) ? Image.FromFile(path) : null;
    }

    private void PopulateStaticChoices()
    {
        _languageCombo.Items.Clear();
        _languageCombo.Items.AddRange(Localizer.Languages.Select(language => new ComboItem<string>(language.Name, language.Code)).ToArray());

        _repeatCombo.Items.Clear();
        _repeatCombo.Items.AddRange(
        [
            new ComboItem<RepeatMode>(_localizer.T("repeat.all"), RepeatMode.All),
            new ComboItem<RepeatMode>(_localizer.T("repeat.one"), RepeatMode.One),
            new ComboItem<RepeatMode>(_localizer.T("repeat.none"), RepeatMode.None)
        ]);

        _displayCombo.Items.Clear();
        for (var i = 0; i < Screen.AllScreens.Length; i++)
        {
            var screen = Screen.AllScreens[i];
            _displayCombo.Items.Add(new ComboItem<int>($"{_localizer.T("display")} {i}: {screen.Bounds.Width}x{screen.Bounds.Height}", i));
        }

        _hardwareCombo.Items.Clear();
        _hardwareCombo.Items.AddRange(
        [
            new ComboItem<HardwareAccelerationMode>(_localizer.T("auto"), HardwareAccelerationMode.Auto),
            new ComboItem<HardwareAccelerationMode>(_localizer.T("disabled"), HardwareAccelerationMode.Disabled),
            new ComboItem<HardwareAccelerationMode>("D3D11VA", HardwareAccelerationMode.D3D11VA),
            new ComboItem<HardwareAccelerationMode>("DXVA2", HardwareAccelerationMode.DXVA2)
        ]);

        _videoOutputCombo.Items.Clear();
        _videoOutputCombo.Items.AddRange(
        [
            new ComboItem<VideoOutputMode>(_localizer.T("auto"), VideoOutputMode.Auto),
            new ComboItem<VideoOutputMode>("Direct3D 11", VideoOutputMode.Direct3D11),
            new ComboItem<VideoOutputMode>("Direct3D 9", VideoOutputMode.Direct3D9),
            new ComboItem<VideoOutputMode>("OpenGL", VideoOutputMode.OpenGL)
        ]);

        _gpuCombo.Items.Clear();
        PopulateGpuChoices();
    }

    private void addVideosButton_Click(object? sender, EventArgs e) => AddVideos();

    private void addFolderButton_Click(object? sender, EventArgs e) => AddFolder();

    private void removeSelectedButton_Click(object? sender, EventArgs e) => RemoveSelected();

    private void clearButton_Click(object? sender, EventArgs e) => ClearPlaylist();

    private void moveUpButton_Click(object? sender, EventArgs e) => MoveSelectedUp();

    private void moveDownButton_Click(object? sender, EventArgs e) => MoveSelectedDown();

    private void saveSettingsButton_Click(object? sender, EventArgs e) => SaveSettingsOnly();

    private void playButton_Click(object? sender, EventArgs e) => StartPlayback();

    private void exitButton_Click(object? sender, EventArgs e) => Close();

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(18),
            BackColor = BackColor
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var playlistPanel = BuildPlaylistPanel();
        var settingsPanel = BuildSettingsPanel();

        var brandPanel = BuildBrandPanel();
        root.Controls.Add(brandPanel, 0, 0);
        root.SetColumnSpan(brandPanel, 2);
        root.Controls.Add(playlistPanel, 0, 1);
        root.Controls.Add(settingsPanel, 1, 1);
        Controls.Add(root);
    }

    private Control BuildBrandPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            RowCount = 1,
            Height = 128,
            Padding = new Padding(0, 0, 0, 18)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));

        var logo = BrandImage("brand-logo.png", 86);
        var model = BrandImage("brand-model.png", 118);

        var titlePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(8, 18, 8, 0)
        };

        titlePanel.Controls.Add(new Label
        {
            Text = _localizer.T("app.title"),
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 24F),
            AutoSize = true
        }, 0, 0);

        titlePanel.Controls.Add(new Label
        {
            Text = _localizer.T("brand.subtitle"),
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(185, 225, 232),
            Font = new Font("Segoe UI", 10F),
            AutoSize = true
        }, 0, 1);

        panel.Controls.Add(logo, 0, 0);
        panel.Controls.Add(titlePanel, 1, 0);
        panel.Controls.Add(model, 2, 0);

        return panel;
    }

    private static PictureBox BrandImage(string fileName, int size)
    {
        var picture = new PictureBox
        {
            Width = size,
            Height = size,
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        var path = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
        if (File.Exists(path))
        {
            picture.Image = Image.FromFile(path);
        }

        return picture;
    }

    private Control BuildPlaylistPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            Padding = new Padding(0, 0, 14, 0)
        };

        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        panel.Controls.Add(Header(_localizer.T("playlist")), 0, 0);

        _playlistList.Dock = DockStyle.Fill;
        _playlistList.BackColor = Color.FromArgb(16, 18, 23);
        _playlistList.ForeColor = Color.White;
        _playlistList.BorderStyle = BorderStyle.FixedSingle;
        _playlistList.HorizontalScrollbar = true;
        _playlistList.SelectionMode = SelectionMode.MultiExtended;
        panel.Controls.Add(_playlistList, 0, 1);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 10, 0, 0),
            AutoSize = true
        };

        buttons.Controls.Add(ActionButton(_localizer.T("add.videos"), AddVideos));
        buttons.Controls.Add(ActionButton(_localizer.T("add.folder"), AddFolder));
        buttons.Controls.Add(ActionButton(_localizer.T("remove.selected"), RemoveSelected));
        buttons.Controls.Add(ActionButton(_localizer.T("clear"), ClearPlaylist));
        buttons.Controls.Add(ActionButton(_localizer.T("move.up"), MoveSelectedUp));
        buttons.Controls.Add(ActionButton(_localizer.T("move.down"), MoveSelectedDown));

        panel.Controls.Add(buttons, 0, 2);

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.ForeColor = Color.FromArgb(190, 198, 210);
        _statusLabel.Padding = new Padding(2, 8, 0, 0);
        panel.Controls.Add(_statusLabel, 0, 3);

        return panel;
    }

    private Control BuildSettingsPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 16,
            ColumnCount = 1,
            Padding = new Padding(14, 0, 0, 0)
        };

        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(Header(_localizer.T("settings")), 0, 0);

        _languageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _languageCombo.Items.AddRange(Localizer.Languages.Select(language => new ComboItem<string>(language.Name, language.Code)).ToArray());

        _repeatCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _repeatCombo.Items.AddRange(
        [
            new ComboItem<RepeatMode>(_localizer.T("repeat.all"), RepeatMode.All),
            new ComboItem<RepeatMode>(_localizer.T("repeat.one"), RepeatMode.One),
            new ComboItem<RepeatMode>(_localizer.T("repeat.none"), RepeatMode.None)
        ]);

        _displayCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        for (var i = 0; i < Screen.AllScreens.Length; i++)
        {
            var screen = Screen.AllScreens[i];
            _displayCombo.Items.Add(new ComboItem<int>($"{_localizer.T("display")} {i}: {screen.Bounds.Width}x{screen.Bounds.Height}", i));
        }

        _hardwareCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _hardwareCombo.Items.AddRange(
        [
            new ComboItem<HardwareAccelerationMode>(_localizer.T("auto"), HardwareAccelerationMode.Auto),
            new ComboItem<HardwareAccelerationMode>(_localizer.T("disabled"), HardwareAccelerationMode.Disabled),
            new ComboItem<HardwareAccelerationMode>("D3D11VA", HardwareAccelerationMode.D3D11VA),
            new ComboItem<HardwareAccelerationMode>("DXVA2", HardwareAccelerationMode.DXVA2)
        ]);

        _videoOutputCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _videoOutputCombo.Items.AddRange(
        [
            new ComboItem<VideoOutputMode>(_localizer.T("auto"), VideoOutputMode.Auto),
            new ComboItem<VideoOutputMode>("Direct3D 11", VideoOutputMode.Direct3D11),
            new ComboItem<VideoOutputMode>("Direct3D 9", VideoOutputMode.Direct3D9),
            new ComboItem<VideoOutputMode>("OpenGL", VideoOutputMode.OpenGL)
        ]);

        _gpuCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        PopulateGpuChoices();

        _cacheInput.Minimum = 100;
        _cacheInput.Maximum = 10000;
        _cacheInput.Increment = 100;

        _shuffleCheck.Text = _localizer.T("shuffle");
        _fullscreenCheck.Text = _localizer.T("fullscreen");
        _kioskCheck.Text = _localizer.T("kiosk");
        _mutedCheck.Text = _localizer.T("muted");

        _gpuInfoLabel.Dock = DockStyle.Top;
        _gpuInfoLabel.AutoSize = true;
        _gpuInfoLabel.ForeColor = Color.FromArgb(158, 168, 184);
        _gpuInfoLabel.Padding = new Padding(0, 6, 0, 0);
        _gpuInfoLabel.Text = BuildGpuInfoText();

        panel.Controls.Add(Field(_localizer.T("language"), _languageCombo), 0, 1);
        panel.Controls.Add(Field(_localizer.T("repeat.mode"), _repeatCombo), 0, 2);
        panel.Controls.Add(CheckRow(_shuffleCheck, _fullscreenCheck), 0, 3);
        panel.Controls.Add(CheckRow(_kioskCheck, _mutedCheck), 0, 4);
        panel.Controls.Add(Field(_localizer.T("target.display"), _displayCombo), 0, 5);
        panel.Controls.Add(Field(_localizer.T("hardware.acceleration"), _hardwareCombo), 0, 6);
        panel.Controls.Add(Field(_localizer.T("video.output"), _videoOutputCombo), 0, 7);
        panel.Controls.Add(Field(_localizer.T("gpu.preference"), _gpuCombo), 0, 8);
        panel.Controls.Add(_gpuInfoLabel, 0, 9);
        panel.Controls.Add(Field(_localizer.T("cache"), _cacheInput), 0, 10);
        panel.Controls.Add(InfoText(_localizer), 0, 11);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 18, 0, 0),
            AutoSize = true
        };

        actions.Controls.Add(PrimaryButton(_localizer.T("play"), StartPlayback));
        actions.Controls.Add(ActionButton(_localizer.T("save.settings"), SaveSettingsOnly));
        actions.Controls.Add(ActionButton(_localizer.T("exit"), Close));

        panel.Controls.Add(actions, 0, 12);
        return panel;
    }

    private static Label Header(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Top,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 16F),
            ForeColor = Color.White,
            Padding = new Padding(0, 0, 0, 10)
        };
    }

    private static Control Field(string labelText, Control editor)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(0, 8, 0, 0),
            AutoSize = true
        };

        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Top,
            AutoSize = true,
            ForeColor = Color.FromArgb(190, 198, 210)
        };

        editor.Dock = DockStyle.Top;
        editor.Height = 34;
        panel.Controls.Add(label, 0, 0);
        panel.Controls.Add(editor, 0, 1);
        return panel;
    }

    private static Control CheckRow(params CheckBox[] checks)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 0)
        };

        foreach (var check in checks)
        {
            check.AutoSize = true;
            check.ForeColor = Color.White;
            check.Margin = new Padding(0, 0, 18, 0);
            panel.Controls.Add(check);
        }

        return panel;
    }

    private static Label InfoText(Localizer localizer)
    {
        return new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ForeColor = Color.FromArgb(158, 168, 184),
            Padding = new Padding(0, 12, 0, 0),
            Text = localizer.T("gpu.note")
        };
    }

    private static Button ActionButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 34,
            Margin = new Padding(0, 0, 8, 8),
            BackColor = Color.FromArgb(50, 55, 66),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        button.FlatAppearance.BorderColor = Color.FromArgb(72, 80, 96);
        button.Click += (_, _) => action();
        return button;
    }

    private static Button PrimaryButton(string text, Action action)
    {
        var button = ActionButton(text, action);
        button.BackColor = Color.FromArgb(26, 115, 232);
        button.FlatAppearance.BorderColor = Color.FromArgb(61, 137, 245);
        button.Width = 120;
        return button;
    }

    private void PopulateGpuChoices()
    {
        _gpuCombo.Items.Add(new ComboItem<GpuPreferenceMode>(_localizer.T("gpu.default"), GpuPreferenceMode.WindowsDefault));
        _gpuCombo.Items.Add(new ComboItem<GpuPreferenceMode>(_localizer.T("gpu.power"), GpuPreferenceMode.PowerSaving));
        _gpuCombo.Items.Add(new ComboItem<GpuPreferenceMode>(_localizer.T("gpu.high"), GpuPreferenceMode.HighPerformance));
    }

    private void LoadSettingsIntoForm()
    {
        _playlistList.Items.Clear();
        foreach (var file in _settings.Playlist.Where(File.Exists))
        {
            _playlistList.Items.Add(file);
        }

        SelectComboValue(_languageCombo, _settings.Language);
        SelectComboValue(_repeatCombo, _settings.RepeatMode);
        SelectComboValue(_displayCombo, Math.Clamp(_settings.DisplayIndex, 0, Math.Max(Screen.AllScreens.Length - 1, 0)));
        SelectComboValue(_hardwareCombo, _settings.HardwareAcceleration);
        SelectComboValue(_videoOutputCombo, _settings.VideoOutput);
        SelectGpuChoice();

        _shuffleCheck.Checked = _settings.Shuffle;
        _fullscreenCheck.Checked = true;
        _fullscreenCheck.Enabled = false;
        _kioskCheck.Checked = _settings.Kiosk;
        _mutedCheck.Checked = _settings.Muted;
        _cacheInput.Value = Math.Clamp(_settings.FileCachingMs, (int)_cacheInput.Minimum, (int)_cacheInput.Maximum);

        UpdateStatus();
    }

    private void SelectGpuChoice()
    {
        SelectComboValue(_gpuCombo, _settings.GpuPreference);
    }

    private static void SelectComboValue<T>(ComboBox comboBox, T value)
    {
        for (var i = 0; i < comboBox.Items.Count; i++)
        {
            if (comboBox.Items[i] is ComboItem<T> item && EqualityComparer<T>.Default.Equals(item.Value, value))
            {
                comboBox.SelectedIndex = i;
                return;
            }
        }

        if (comboBox.Items.Count > 0)
        {
            comboBox.SelectedIndex = 0;
        }
    }

    private void AddVideos()
    {
        using var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = SupportedVideo.FileDialogFilter(_localizer),
            Title = _localizer.T("video.dialog.title")
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        AddFiles(dialog.FileNames);
    }

    private void AddFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = _localizer.T("folder.dialog.title"),
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        AddFiles(Directory.EnumerateFiles(dialog.SelectedPath, "*.*", SearchOption.AllDirectories));
    }

    private void AddFiles(IEnumerable<string> files)
    {
        var existing = _playlistList.Items.Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var added = 0;

        foreach (var file in files.Where(SupportedVideo.IsSupported).Where(File.Exists).OrderBy(Path.GetFileName))
        {
            if (!existing.Add(file))
            {
                continue;
            }

            _playlistList.Items.Add(file);
            added++;
        }

        SetStatus(_localizer.Format("status.added", added));
        UpdateStatus();
    }

    private void RemoveSelected()
    {
        var indices = _playlistList.SelectedIndices.Cast<int>().OrderDescending().ToList();
        foreach (var index in indices)
        {
            _playlistList.Items.RemoveAt(index);
        }

        UpdateStatus();
    }

    private void ClearPlaylist()
    {
        _playlistList.Items.Clear();
        UpdateStatus();
    }

    private void MoveSelectedUp()
    {
        if (_playlistList.SelectedIndices.Count == 0)
        {
            return;
        }

        var selected = _playlistList.SelectedIndices.Cast<int>().Order().ToList();
        if (selected[0] == 0)
        {
            return;
        }

        foreach (var index in selected)
        {
            var item = _playlistList.Items[index];
            _playlistList.Items.RemoveAt(index);
            _playlistList.Items.Insert(index - 1, item);
        }

        _playlistList.ClearSelected();
        foreach (var index in selected)
        {
            _playlistList.SetSelected(index - 1, true);
        }
    }

    private void MoveSelectedDown()
    {
        if (_playlistList.SelectedIndices.Count == 0)
        {
            return;
        }

        var selected = _playlistList.SelectedIndices.Cast<int>().OrderDescending().ToList();
        if (selected[0] == _playlistList.Items.Count - 1)
        {
            return;
        }

        foreach (var index in selected)
        {
            var item = _playlistList.Items[index];
            _playlistList.Items.RemoveAt(index);
            _playlistList.Items.Insert(index + 1, item);
        }

        _playlistList.ClearSelected();
        foreach (var index in selected)
        {
            _playlistList.SetSelected(index + 1, true);
        }
    }

    private void SaveSettingsOnly()
    {
        var settings = ReadSettingsFromForm();
        SettingsStore.Save(settings);
        var gpuMessage = GpuService.ApplyWindowsGpuPreference(settings, _localizer);
        SetStatus($"{_localizer.T("settings.saved")} {gpuMessage}");
    }

    private void StartPlayback()
    {
        var settings = ReadSettingsFromForm();
        SettingsStore.Save(settings);
        var gpuMessage = GpuService.ApplyWindowsGpuPreference(settings, _localizer);

        if (settings.Playlist.Count == 0)
        {
            var result = MessageBox.Show(
                _localizer.T("empty.playlist.message"),
                _localizer.T("empty.playlist.title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }
        }

        SetStatus(gpuMessage);
        Hide();
        using var player = new VideoWallForm(settings);
        player.ShowDialog(this);
        Show();
        Activate();
    }

    private AppSettings ReadSettingsFromForm()
    {
        var gpuChoice = ((ComboItem<GpuPreferenceMode>)_gpuCombo.SelectedItem!).Value;

        return new AppSettings
        {
            Playlist = _playlistList.Items.Cast<string>().Where(File.Exists).ToList(),
            Language = ((ComboItem<string>)_languageCombo.SelectedItem!).Value,
            RepeatMode = ((ComboItem<RepeatMode>)_repeatCombo.SelectedItem!).Value,
            Shuffle = _shuffleCheck.Checked,
            Fullscreen = true,
            Kiosk = _kioskCheck.Checked,
            Muted = _mutedCheck.Checked,
            DisplayIndex = ((ComboItem<int>)_displayCombo.SelectedItem!).Value,
            HardwareAcceleration = ((ComboItem<HardwareAccelerationMode>)_hardwareCombo.SelectedItem!).Value,
            VideoOutput = ((ComboItem<VideoOutputMode>)_videoOutputCombo.SelectedItem!).Value,
            GpuPreference = gpuChoice,
            NamedGpu = null,
            FileCachingMs = (int)_cacheInput.Value,
            VideoDirectory = _settings.VideoDirectory
        };
    }

    private void UpdateStatus()
    {
        SetStatus(_localizer.Format("status.count", _playlistList.Items.Count));
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = message;
    }

    private string BuildGpuInfoText()
    {
        var gpus = GpuService.GetDetectedGpus();
        if (gpus.Count == 0)
        {
            return $"{_localizer.T("gpu.detected")}: -";
        }

        return $"{_localizer.T("gpu.detected")}: " + string.Join(", ", gpus.Select(gpu => gpu.Name));
    }
}
