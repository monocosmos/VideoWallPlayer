namespace VideoWallPlayer.Native;

partial class LauncherForm
{
    private System.ComponentModel.IContainer components = null!;

    private TableLayoutPanel rootTableLayoutPanel = null!;
    private TableLayoutPanel brandTableLayoutPanel = null!;
    private PictureBox logoPictureBox = null!;
    private TableLayoutPanel titleTableLayoutPanel = null!;
    private Label titleLabel = null!;
    private Label subtitleLabel = null!;
    private PictureBox modelPictureBox = null!;
    private TableLayoutPanel playlistTableLayoutPanel = null!;
    private Label playlistHeaderLabel = null!;
    private ListBox playlistListBox = null!;
    private FlowLayoutPanel playlistButtonsFlowLayoutPanel = null!;
    private Button addVideosButton = null!;
    private Button addFolderButton = null!;
    private Button removeSelectedButton = null!;
    private Button clearButton = null!;
    private Button moveUpButton = null!;
    private Button moveDownButton = null!;
    private Label statusLabel = null!;
    private TableLayoutPanel settingsTableLayoutPanel = null!;
    private Label settingsHeaderLabel = null!;
    private Label languageLabel = null!;
    private ComboBox languageComboBox = null!;
    private Label repeatLabel = null!;
    private ComboBox repeatComboBox = null!;
    private FlowLayoutPanel playbackOptionsFlowLayoutPanel = null!;
    private CheckBox shuffleCheckBox = null!;
    private CheckBox fullscreenCheckBox = null!;
    private CheckBox kioskCheckBox = null!;
    private CheckBox mutedCheckBox = null!;
    private Label displayLabel = null!;
    private ComboBox displayComboBox = null!;
    private Label hardwareLabel = null!;
    private ComboBox hardwareComboBox = null!;
    private Label outputLabel = null!;
    private ComboBox videoOutputComboBox = null!;
    private Label gpuLabel = null!;
    private ComboBox gpuComboBox = null!;
    private Label gpuInfoLabel = null!;
    private Label cacheLabel = null!;
    private NumericUpDown cacheNumericUpDown = null!;
    private Label gpuNoteLabel = null!;
    private FlowLayoutPanel actionsFlowLayoutPanel = null!;
    private Button playButton = null!;
    private Button saveSettingsButton = null!;
    private Button exitButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            logoPictureBox?.Image?.Dispose();
            modelPictureBox?.Image?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        rootTableLayoutPanel = new TableLayoutPanel();
        brandTableLayoutPanel = new TableLayoutPanel();
        logoPictureBox = new PictureBox();
        titleTableLayoutPanel = new TableLayoutPanel();
        titleLabel = new Label();
        subtitleLabel = new Label();
        modelPictureBox = new PictureBox();
        playlistTableLayoutPanel = new TableLayoutPanel();
        playlistHeaderLabel = new Label();
        playlistListBox = new ListBox();
        playlistButtonsFlowLayoutPanel = new FlowLayoutPanel();
        addVideosButton = new Button();
        addFolderButton = new Button();
        removeSelectedButton = new Button();
        clearButton = new Button();
        moveUpButton = new Button();
        moveDownButton = new Button();
        statusLabel = new Label();
        settingsTableLayoutPanel = new TableLayoutPanel();
        settingsHeaderLabel = new Label();
        actionsFlowLayoutPanel = new FlowLayoutPanel();
        playButton = new Button();
        saveSettingsButton = new Button();
        exitButton = new Button();
        languageLabel = new Label();
        languageComboBox = new ComboBox();
        repeatLabel = new Label();
        repeatComboBox = new ComboBox();
        playbackOptionsFlowLayoutPanel = new FlowLayoutPanel();
        shuffleCheckBox = new CheckBox();
        fullscreenCheckBox = new CheckBox();
        kioskCheckBox = new CheckBox();
        mutedCheckBox = new CheckBox();
        displayLabel = new Label();
        displayComboBox = new ComboBox();
        hardwareLabel = new Label();
        hardwareComboBox = new ComboBox();
        outputLabel = new Label();
        videoOutputComboBox = new ComboBox();
        gpuLabel = new Label();
        gpuComboBox = new ComboBox();
        gpuInfoLabel = new Label();
        cacheLabel = new Label();
        cacheNumericUpDown = new NumericUpDown();
        gpuNoteLabel = new Label();
        rootTableLayoutPanel.SuspendLayout();
        brandTableLayoutPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
        titleTableLayoutPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)modelPictureBox).BeginInit();
        playlistTableLayoutPanel.SuspendLayout();
        playlistButtonsFlowLayoutPanel.SuspendLayout();
        settingsTableLayoutPanel.SuspendLayout();
        actionsFlowLayoutPanel.SuspendLayout();
        playbackOptionsFlowLayoutPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)cacheNumericUpDown).BeginInit();
        SuspendLayout();
        // 
        // rootTableLayoutPanel
        // 
        rootTableLayoutPanel.BackColor = Color.FromArgb(24, 26, 31);
        rootTableLayoutPanel.ColumnCount = 2;
        rootTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
        rootTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
        rootTableLayoutPanel.Controls.Add(brandTableLayoutPanel, 0, 0);
        rootTableLayoutPanel.Controls.Add(playlistTableLayoutPanel, 0, 1);
        rootTableLayoutPanel.Controls.Add(settingsTableLayoutPanel, 1, 1);
        rootTableLayoutPanel.Dock = DockStyle.Fill;
        rootTableLayoutPanel.Location = new Point(0, 0);
        rootTableLayoutPanel.Name = "rootTableLayoutPanel";
        rootTableLayoutPanel.Padding = new Padding(24);
        rootTableLayoutPanel.RowCount = 2;
        rootTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
        rootTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootTableLayoutPanel.Size = new Size(1264, 800);
        rootTableLayoutPanel.TabIndex = 0;
        // 
        // brandTableLayoutPanel
        // 
        brandTableLayoutPanel.ColumnCount = 3;
        rootTableLayoutPanel.SetColumnSpan(brandTableLayoutPanel, 2);
        brandTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
        brandTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        brandTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 148F));
        brandTableLayoutPanel.Controls.Add(logoPictureBox, 0, 0);
        brandTableLayoutPanel.Controls.Add(titleTableLayoutPanel, 1, 0);
        brandTableLayoutPanel.Controls.Add(modelPictureBox, 2, 0);
        brandTableLayoutPanel.Dock = DockStyle.Fill;
        brandTableLayoutPanel.Location = new Point(24, 24);
        brandTableLayoutPanel.Margin = new Padding(0, 0, 0, 18);
        brandTableLayoutPanel.Name = "brandTableLayoutPanel";
        brandTableLayoutPanel.RowCount = 1;
        brandTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        brandTableLayoutPanel.Size = new Size(1216, 132);
        brandTableLayoutPanel.TabIndex = 0;
        // 
        // logoPictureBox
        // 
        logoPictureBox.Dock = DockStyle.Fill;
        logoPictureBox.Location = new Point(0, 0);
        logoPictureBox.Margin = new Padding(0);
        logoPictureBox.Name = "logoPictureBox";
        logoPictureBox.Size = new Size(118, 132);
        logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        logoPictureBox.TabIndex = 0;
        logoPictureBox.TabStop = false;
        // 
        // titleTableLayoutPanel
        // 
        titleTableLayoutPanel.ColumnCount = 1;
        titleTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        titleTableLayoutPanel.Controls.Add(titleLabel, 0, 0);
        titleTableLayoutPanel.Controls.Add(subtitleLabel, 0, 1);
        titleTableLayoutPanel.Dock = DockStyle.Fill;
        titleTableLayoutPanel.Location = new Point(126, 18);
        titleTableLayoutPanel.Margin = new Padding(8, 18, 8, 0);
        titleTableLayoutPanel.Name = "titleTableLayoutPanel";
        titleTableLayoutPanel.RowCount = 2;
        titleTableLayoutPanel.RowStyles.Add(new RowStyle());
        titleTableLayoutPanel.RowStyles.Add(new RowStyle());
        titleTableLayoutPanel.Size = new Size(934, 114);
        titleTableLayoutPanel.TabIndex = 1;
        // 
        // titleLabel
        // 
        titleLabel.AutoSize = true;
        titleLabel.Dock = DockStyle.Top;
        titleLabel.Font = new Font("Segoe UI Semibold", 28F);
        titleLabel.ForeColor = Color.White;
        titleLabel.Location = new Point(0, 0);
        titleLabel.Margin = new Padding(0);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(934, 51);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "VideoWallPlayer";
        // 
        // subtitleLabel
        // 
        subtitleLabel.AutoSize = true;
        subtitleLabel.Dock = DockStyle.Top;
        subtitleLabel.ForeColor = Color.FromArgb(185, 225, 232);
        subtitleLabel.Location = new Point(0, 57);
        subtitleLabel.Margin = new Padding(0, 6, 0, 0);
        subtitleLabel.Name = "subtitleLabel";
        subtitleLabel.Size = new Size(934, 20);
        subtitleLabel.TabIndex = 1;
        subtitleLabel.Text = "Profesyonel video wall oynatma";
        // 
        // modelPictureBox
        // 
        modelPictureBox.Dock = DockStyle.Fill;
        modelPictureBox.Location = new Point(1068, 0);
        modelPictureBox.Margin = new Padding(0);
        modelPictureBox.Name = "modelPictureBox";
        modelPictureBox.Size = new Size(148, 132);
        modelPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        modelPictureBox.TabIndex = 2;
        modelPictureBox.TabStop = false;
        // 
        // playlistTableLayoutPanel
        // 
        playlistTableLayoutPanel.ColumnCount = 1;
        playlistTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        playlistTableLayoutPanel.Controls.Add(playlistHeaderLabel, 0, 0);
        playlistTableLayoutPanel.Controls.Add(playlistListBox, 0, 1);
        playlistTableLayoutPanel.Controls.Add(playlistButtonsFlowLayoutPanel, 0, 2);
        playlistTableLayoutPanel.Controls.Add(statusLabel, 0, 3);
        playlistTableLayoutPanel.Dock = DockStyle.Fill;
        playlistTableLayoutPanel.Location = new Point(24, 174);
        playlistTableLayoutPanel.Margin = new Padding(0, 0, 14, 0);
        playlistTableLayoutPanel.Name = "playlistTableLayoutPanel";
        playlistTableLayoutPanel.RowCount = 4;
        playlistTableLayoutPanel.RowStyles.Add(new RowStyle());
        playlistTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        playlistTableLayoutPanel.RowStyles.Add(new RowStyle());
        playlistTableLayoutPanel.RowStyles.Add(new RowStyle());
        playlistTableLayoutPanel.Size = new Size(739, 602);
        playlistTableLayoutPanel.TabIndex = 1;
        // 
        // playlistHeaderLabel
        // 
        playlistHeaderLabel.AutoSize = true;
        playlistHeaderLabel.Dock = DockStyle.Top;
        playlistHeaderLabel.Font = new Font("Segoe UI Semibold", 18F);
        playlistHeaderLabel.ForeColor = Color.White;
        playlistHeaderLabel.Location = new Point(0, 0);
        playlistHeaderLabel.Margin = new Padding(0, 0, 0, 10);
        playlistHeaderLabel.Name = "playlistHeaderLabel";
        playlistHeaderLabel.Size = new Size(739, 32);
        playlistHeaderLabel.TabIndex = 0;
        playlistHeaderLabel.Text = "Oynatma Listesi";
        // 
        // playlistListBox
        // 
        playlistListBox.BackColor = Color.FromArgb(16, 18, 23);
        playlistListBox.BorderStyle = BorderStyle.FixedSingle;
        playlistListBox.Dock = DockStyle.Fill;
        playlistListBox.ForeColor = Color.White;
        playlistListBox.HorizontalScrollbar = true;
        playlistListBox.ItemHeight = 20;
        playlistListBox.Location = new Point(0, 42);
        playlistListBox.Margin = new Padding(0);
        playlistListBox.Name = "playlistListBox";
        playlistListBox.SelectionMode = SelectionMode.MultiExtended;
        playlistListBox.Size = new Size(739, 493);
        playlistListBox.TabIndex = 1;
        // 
        // playlistButtonsFlowLayoutPanel
        // 
        playlistButtonsFlowLayoutPanel.AutoSize = true;
        playlistButtonsFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        playlistButtonsFlowLayoutPanel.Controls.Add(addVideosButton);
        playlistButtonsFlowLayoutPanel.Controls.Add(addFolderButton);
        playlistButtonsFlowLayoutPanel.Controls.Add(removeSelectedButton);
        playlistButtonsFlowLayoutPanel.Controls.Add(clearButton);
        playlistButtonsFlowLayoutPanel.Controls.Add(moveUpButton);
        playlistButtonsFlowLayoutPanel.Controls.Add(moveDownButton);
        playlistButtonsFlowLayoutPanel.Dock = DockStyle.Fill;
        playlistButtonsFlowLayoutPanel.Location = new Point(0, 545);
        playlistButtonsFlowLayoutPanel.Margin = new Padding(0, 10, 0, 0);
        playlistButtonsFlowLayoutPanel.Name = "playlistButtonsFlowLayoutPanel";
        playlistButtonsFlowLayoutPanel.Size = new Size(739, 29);
        playlistButtonsFlowLayoutPanel.TabIndex = 2;
        // 
        // addVideosButton
        // 
        addVideosButton.Location = new Point(3, 3);
        addVideosButton.Name = "addVideosButton";
        addVideosButton.Size = new Size(75, 23);
        addVideosButton.TabIndex = 0;
        addVideosButton.Text = "Video Ekle";
        addVideosButton.Click += addVideosButton_Click;
        // 
        // addFolderButton
        // 
        addFolderButton.Location = new Point(84, 3);
        addFolderButton.Name = "addFolderButton";
        addFolderButton.Size = new Size(75, 23);
        addFolderButton.TabIndex = 1;
        addFolderButton.Text = "Klasör Ekle";
        addFolderButton.Click += addFolderButton_Click;
        // 
        // removeSelectedButton
        // 
        removeSelectedButton.Location = new Point(165, 3);
        removeSelectedButton.Name = "removeSelectedButton";
        removeSelectedButton.Size = new Size(75, 23);
        removeSelectedButton.TabIndex = 2;
        removeSelectedButton.Text = "Seçileni Sil";
        removeSelectedButton.Click += removeSelectedButton_Click;
        // 
        // clearButton
        // 
        clearButton.Location = new Point(246, 3);
        clearButton.Name = "clearButton";
        clearButton.Size = new Size(75, 23);
        clearButton.TabIndex = 3;
        clearButton.Text = "Temizle";
        clearButton.Click += clearButton_Click;
        // 
        // moveUpButton
        // 
        moveUpButton.Location = new Point(327, 3);
        moveUpButton.Name = "moveUpButton";
        moveUpButton.Size = new Size(75, 23);
        moveUpButton.TabIndex = 4;
        moveUpButton.Text = "Yukarı";
        moveUpButton.Click += moveUpButton_Click;
        // 
        // moveDownButton
        // 
        moveDownButton.Location = new Point(408, 3);
        moveDownButton.Name = "moveDownButton";
        moveDownButton.Size = new Size(75, 23);
        moveDownButton.TabIndex = 5;
        moveDownButton.Text = "Aşağı";
        moveDownButton.Click += moveDownButton_Click;
        // 
        // statusLabel
        // 
        statusLabel.AutoSize = true;
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.ForeColor = Color.FromArgb(190, 198, 210);
        statusLabel.Location = new Point(2, 582);
        statusLabel.Margin = new Padding(2, 8, 0, 0);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(737, 20);
        statusLabel.TabIndex = 3;
        statusLabel.Text = "0 video listede.";
        // 
        // settingsTableLayoutPanel
        // 
        settingsTableLayoutPanel.AutoScroll = true;
        settingsTableLayoutPanel.ColumnCount = 1;
        settingsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        settingsTableLayoutPanel.Controls.Add(settingsHeaderLabel, 0, 0);
        settingsTableLayoutPanel.Controls.Add(actionsFlowLayoutPanel, 0, 1);
        settingsTableLayoutPanel.Controls.Add(languageLabel, 0, 2);
        settingsTableLayoutPanel.Controls.Add(languageComboBox, 0, 3);
        settingsTableLayoutPanel.Controls.Add(repeatLabel, 0, 4);
        settingsTableLayoutPanel.Controls.Add(repeatComboBox, 0, 5);
        settingsTableLayoutPanel.Controls.Add(playbackOptionsFlowLayoutPanel, 0, 6);
        settingsTableLayoutPanel.Controls.Add(displayLabel, 0, 7);
        settingsTableLayoutPanel.Controls.Add(displayComboBox, 0, 8);
        settingsTableLayoutPanel.Controls.Add(hardwareLabel, 0, 9);
        settingsTableLayoutPanel.Controls.Add(hardwareComboBox, 0, 10);
        settingsTableLayoutPanel.Controls.Add(outputLabel, 0, 11);
        settingsTableLayoutPanel.Controls.Add(videoOutputComboBox, 0, 12);
        settingsTableLayoutPanel.Controls.Add(gpuLabel, 0, 13);
        settingsTableLayoutPanel.Controls.Add(gpuComboBox, 0, 14);
        settingsTableLayoutPanel.Controls.Add(gpuInfoLabel, 0, 15);
        settingsTableLayoutPanel.Controls.Add(cacheLabel, 0, 16);
        settingsTableLayoutPanel.Controls.Add(cacheNumericUpDown, 0, 17);
        settingsTableLayoutPanel.Controls.Add(gpuNoteLabel, 0, 18);
        settingsTableLayoutPanel.Dock = DockStyle.Fill;
        settingsTableLayoutPanel.Location = new Point(791, 174);
        settingsTableLayoutPanel.Margin = new Padding(14, 0, 0, 0);
        settingsTableLayoutPanel.Name = "settingsTableLayoutPanel";
        settingsTableLayoutPanel.RowCount = 19;
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle());
        settingsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        settingsTableLayoutPanel.Size = new Size(449, 602);
        settingsTableLayoutPanel.TabIndex = 2;
        // 
        // settingsHeaderLabel
        // 
        settingsHeaderLabel.AutoSize = true;
        settingsHeaderLabel.Font = new Font("Segoe UI Semibold", 18F);
        settingsHeaderLabel.ForeColor = Color.White;
        settingsHeaderLabel.Location = new Point(0, 0);
        settingsHeaderLabel.Margin = new Padding(0, 0, 0, 10);
        settingsHeaderLabel.Name = "settingsHeaderLabel";
        settingsHeaderLabel.Size = new Size(92, 32);
        settingsHeaderLabel.TabIndex = 0;
        settingsHeaderLabel.Text = "Ayarlar";
        // 
        // actionsFlowLayoutPanel
        // 
        actionsFlowLayoutPanel.AutoSize = true;
        actionsFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        actionsFlowLayoutPanel.Controls.Add(playButton);
        actionsFlowLayoutPanel.Controls.Add(saveSettingsButton);
        actionsFlowLayoutPanel.Controls.Add(exitButton);
        actionsFlowLayoutPanel.Dock = DockStyle.Fill;
        actionsFlowLayoutPanel.Location = new Point(0, 42);
        actionsFlowLayoutPanel.Margin = new Padding(0, 0, 0, 14);
        actionsFlowLayoutPanel.Name = "actionsFlowLayoutPanel";
        actionsFlowLayoutPanel.Size = new Size(449, 29);
        actionsFlowLayoutPanel.TabIndex = 1;
        // 
        // playButton
        // 
        playButton.Location = new Point(3, 3);
        playButton.Name = "playButton";
        playButton.Size = new Size(75, 23);
        playButton.TabIndex = 0;
        playButton.Text = "Videoyu Başlat";
        playButton.Click += playButton_Click;
        // 
        // saveSettingsButton
        // 
        saveSettingsButton.Location = new Point(84, 3);
        saveSettingsButton.Name = "saveSettingsButton";
        saveSettingsButton.Size = new Size(75, 23);
        saveSettingsButton.TabIndex = 1;
        saveSettingsButton.Text = "Ayar Kaydet";
        saveSettingsButton.Click += saveSettingsButton_Click;
        // 
        // exitButton
        // 
        exitButton.Location = new Point(165, 3);
        exitButton.Name = "exitButton";
        exitButton.Size = new Size(75, 23);
        exitButton.TabIndex = 2;
        exitButton.Text = "Çıkış";
        exitButton.Click += exitButton_Click;
        // 
        // languageLabel
        // 
        languageLabel.Location = new Point(3, 85);
        languageLabel.Name = "languageLabel";
        languageLabel.Size = new Size(100, 23);
        languageLabel.TabIndex = 2;
        languageLabel.Text = "Dil";
        // 
        // languageComboBox
        // 
        languageComboBox.Location = new Point(3, 111);
        languageComboBox.Name = "languageComboBox";
        languageComboBox.Size = new Size(121, 28);
        languageComboBox.TabIndex = 3;
        // 
        // repeatLabel
        // 
        repeatLabel.Location = new Point(3, 142);
        repeatLabel.Name = "repeatLabel";
        repeatLabel.Size = new Size(100, 23);
        repeatLabel.TabIndex = 4;
        repeatLabel.Text = "Tekrar modu";
        // 
        // repeatComboBox
        // 
        repeatComboBox.Location = new Point(3, 168);
        repeatComboBox.Name = "repeatComboBox";
        repeatComboBox.Size = new Size(121, 28);
        repeatComboBox.TabIndex = 5;
        // 
        // playbackOptionsFlowLayoutPanel
        // 
        playbackOptionsFlowLayoutPanel.AutoSize = true;
        playbackOptionsFlowLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        playbackOptionsFlowLayoutPanel.Controls.Add(shuffleCheckBox);
        playbackOptionsFlowLayoutPanel.Controls.Add(fullscreenCheckBox);
        playbackOptionsFlowLayoutPanel.Controls.Add(kioskCheckBox);
        playbackOptionsFlowLayoutPanel.Controls.Add(mutedCheckBox);
        playbackOptionsFlowLayoutPanel.Dock = DockStyle.Top;
        playbackOptionsFlowLayoutPanel.Location = new Point(0, 209);
        playbackOptionsFlowLayoutPanel.Margin = new Padding(0, 10, 0, 0);
        playbackOptionsFlowLayoutPanel.Name = "playbackOptionsFlowLayoutPanel";
        playbackOptionsFlowLayoutPanel.Size = new Size(449, 30);
        playbackOptionsFlowLayoutPanel.TabIndex = 6;
        // 
        // shuffleCheckBox
        // 
        shuffleCheckBox.Location = new Point(3, 3);
        shuffleCheckBox.Name = "shuffleCheckBox";
        shuffleCheckBox.Size = new Size(104, 24);
        shuffleCheckBox.TabIndex = 0;
        shuffleCheckBox.Text = "Karışık oynat";
        // 
        // fullscreenCheckBox
        // 
        fullscreenCheckBox.Location = new Point(113, 3);
        fullscreenCheckBox.Name = "fullscreenCheckBox";
        fullscreenCheckBox.Size = new Size(104, 24);
        fullscreenCheckBox.TabIndex = 1;
        fullscreenCheckBox.Text = "Tam ekran oynat";
        // 
        // kioskCheckBox
        // 
        kioskCheckBox.Location = new Point(223, 3);
        kioskCheckBox.Name = "kioskCheckBox";
        kioskCheckBox.Size = new Size(104, 24);
        kioskCheckBox.TabIndex = 2;
        kioskCheckBox.Text = "Kiosk modu";
        // 
        // mutedCheckBox
        // 
        mutedCheckBox.Location = new Point(333, 3);
        mutedCheckBox.Name = "mutedCheckBox";
        mutedCheckBox.Size = new Size(104, 24);
        mutedCheckBox.TabIndex = 3;
        mutedCheckBox.Text = "Sessiz";
        // 
        // displayLabel
        // 
        displayLabel.Location = new Point(3, 239);
        displayLabel.Name = "displayLabel";
        displayLabel.Size = new Size(100, 23);
        displayLabel.TabIndex = 7;
        displayLabel.Text = "Hedef ekran";
        // 
        // displayComboBox
        // 
        displayComboBox.Location = new Point(3, 265);
        displayComboBox.Name = "displayComboBox";
        displayComboBox.Size = new Size(121, 28);
        displayComboBox.TabIndex = 8;
        // 
        // hardwareLabel
        // 
        hardwareLabel.Location = new Point(3, 291);
        hardwareLabel.Name = "hardwareLabel";
        hardwareLabel.Size = new Size(100, 23);
        hardwareLabel.TabIndex = 9;
        hardwareLabel.Text = "Donanım hızlandırma";
        // 
        // hardwareComboBox
        // 
        hardwareComboBox.Location = new Point(3, 317);
        hardwareComboBox.Name = "hardwareComboBox";
        hardwareComboBox.Size = new Size(121, 28);
        hardwareComboBox.TabIndex = 10;
        // 
        // outputLabel
        // 
        outputLabel.Location = new Point(3, 343);
        outputLabel.Name = "outputLabel";
        outputLabel.Size = new Size(100, 23);
        outputLabel.TabIndex = 11;
        outputLabel.Text = "Video çıkışı";
        // 
        // videoOutputComboBox
        // 
        videoOutputComboBox.Location = new Point(3, 369);
        videoOutputComboBox.Name = "videoOutputComboBox";
        videoOutputComboBox.Size = new Size(121, 28);
        videoOutputComboBox.TabIndex = 12;
        // 
        // gpuLabel
        // 
        gpuLabel.Location = new Point(3, 395);
        gpuLabel.Name = "gpuLabel";
        gpuLabel.Size = new Size(100, 23);
        gpuLabel.TabIndex = 13;
        gpuLabel.Text = "GPU tercihi";
        // 
        // gpuComboBox
        // 
        gpuComboBox.Location = new Point(3, 421);
        gpuComboBox.Name = "gpuComboBox";
        gpuComboBox.Size = new Size(121, 28);
        gpuComboBox.TabIndex = 14;
        // 
        // gpuInfoLabel
        // 
        gpuInfoLabel.AutoSize = true;
        gpuInfoLabel.Dock = DockStyle.Top;
        gpuInfoLabel.ForeColor = Color.FromArgb(158, 168, 184);
        gpuInfoLabel.Location = new Point(0, 453);
        gpuInfoLabel.Margin = new Padding(0, 6, 0, 0);
        gpuInfoLabel.Name = "gpuInfoLabel";
        gpuInfoLabel.Size = new Size(449, 20);
        gpuInfoLabel.TabIndex = 15;
        gpuInfoLabel.Text = "Algılanan GPU: -";
        // 
        // cacheLabel
        // 
        cacheLabel.Location = new Point(3, 473);
        cacheLabel.Name = "cacheLabel";
        cacheLabel.Size = new Size(100, 23);
        cacheLabel.TabIndex = 16;
        cacheLabel.Text = "Önbellek (ms)";
        // 
        // cacheNumericUpDown
        // 
        cacheNumericUpDown.Dock = DockStyle.Top;
        cacheNumericUpDown.Increment = new decimal(new int[] { 100, 0, 0, 0 });
        cacheNumericUpDown.Location = new Point(0, 500);
        cacheNumericUpDown.Margin = new Padding(0, 4, 0, 8);
        cacheNumericUpDown.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        cacheNumericUpDown.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
        cacheNumericUpDown.Name = "cacheNumericUpDown";
        cacheNumericUpDown.Size = new Size(449, 27);
        cacheNumericUpDown.TabIndex = 17;
        cacheNumericUpDown.Value = new decimal(new int[] { 3000, 0, 0, 0 });
        // 
        // gpuNoteLabel
        // 
        gpuNoteLabel.AutoSize = true;
        gpuNoteLabel.Dock = DockStyle.Top;
        gpuNoteLabel.ForeColor = Color.FromArgb(158, 168, 184);
        gpuNoteLabel.Location = new Point(0, 547);
        gpuNoteLabel.Margin = new Padding(0, 12, 0, 0);
        gpuNoteLabel.Name = "gpuNoteLabel";
        gpuNoteLabel.Size = new Size(449, 20);
        gpuNoteLabel.TabIndex = 18;
        gpuNoteLabel.Text = "GPU tercihi Windows tarafından uygulanır.";
        // 
        // LauncherForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(24, 26, 31);
        ClientSize = new Size(1264, 800);
        Controls.Add(rootTableLayoutPanel);
        Font = new Font("Segoe UI", 11F);
        ForeColor = Color.White;
        MinimumSize = new Size(960, 620);
        Name = "LauncherForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "VideoWallPlayer";
        rootTableLayoutPanel.ResumeLayout(false);
        brandTableLayoutPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
        titleTableLayoutPanel.ResumeLayout(false);
        titleTableLayoutPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)modelPictureBox).EndInit();
        playlistTableLayoutPanel.ResumeLayout(false);
        playlistTableLayoutPanel.PerformLayout();
        playlistButtonsFlowLayoutPanel.ResumeLayout(false);
        settingsTableLayoutPanel.ResumeLayout(false);
        settingsTableLayoutPanel.PerformLayout();
        actionsFlowLayoutPanel.ResumeLayout(false);
        playbackOptionsFlowLayoutPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)cacheNumericUpDown).EndInit();
        ResumeLayout(false);
    }

}
