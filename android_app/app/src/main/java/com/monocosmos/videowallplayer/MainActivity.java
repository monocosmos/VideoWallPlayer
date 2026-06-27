package com.monocosmos.videowallplayer;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.ClipData;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.AssetFileDescriptor;
import android.database.Cursor;
import android.graphics.Color;
import android.graphics.Typeface;
import android.graphics.drawable.GradientDrawable;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.provider.OpenableColumns;
import android.view.Gravity;
import android.view.KeyEvent;
import android.view.View;
import android.view.Window;
import android.view.WindowInsets;
import android.view.WindowInsetsController;
import android.view.WindowManager;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.NumberPicker;
import android.widget.ScrollView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import org.json.JSONArray;
import org.json.JSONException;
import org.videolan.libvlc.LibVLC;
import org.videolan.libvlc.Media;
import org.videolan.libvlc.MediaPlayer;
import org.videolan.libvlc.util.VLCVideoLayout;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Random;
import java.util.Set;

public final class MainActivity extends Activity {
    private static final int RequestVideos = 1001;
    private static final String PrefsName = "videowallplayer.android";
    private static final String KeyPlaylist = "playlist";
    private static final String KeyRepeat = "repeat";
    private static final String KeyShuffle = "shuffle";
    private static final String KeyMuted = "muted";
    private static final String KeyHardware = "hardware";
    private static final String KeyCache = "cache";

    private static final int RepeatAll = 0;
    private static final int RepeatOne = 1;
    private static final int RepeatNone = 2;
    private static final int StartupCachingMs = 350;
    private static final int StandbyFirstFramePauseMs = 160;
    private static final int BackgroundColor = Color.rgb(13, 17, 25);
    private static final int SurfaceColor = Color.rgb(24, 30, 42);
    private static final int SurfaceAltColor = Color.rgb(15, 20, 30);
    private static final int AccentColor = Color.rgb(0, 202, 218);
    private static final int AccentHoverColor = Color.rgb(20, 226, 238);
    private static final int TextMutedColor = Color.rgb(172, 187, 205);

    private final ArrayList<String> playlist = new ArrayList<>();
    private final ArrayList<Integer> playOrder = new ArrayList<>();
    private final Random random = new Random();

    private SharedPreferences prefs;
    private FrameLayout root;
    private LinearLayout launcherView;
    private FrameLayout playerView;
    private VLCVideoLayout activeVideoLayout;
    private VLCVideoLayout standbyVideoLayout;
    private ListView playlistListView;
    private ArrayAdapter<String> playlistAdapter;
    private TextView statusLabel;
    private Spinner repeatSpinner;
    private Spinner hardwareSpinner;
    private CheckBox shuffleCheckBox;
    private CheckBox mutedCheckBox;
    private NumberPicker cachePicker;

    private LibVLC libVlc;
    private MediaPlayer activePlayer;
    private MediaPlayer standbyPlayer;
    private AssetFileDescriptor activeMediaDescriptor;
    private AssetFileDescriptor standbyMediaDescriptor;
    private int orderIndex;
    private int standbyOrderIndex = -1;
    private boolean playerVisible;
    private boolean paused;
    private boolean standbyPlaying;
    private boolean standbyPriming;
    private boolean standbyReady;
    private final Handler mainHandler = new Handler(Looper.getMainLooper());
    private final Runnable freezeStandbyRunnable = this::freezeStandbyOnFirstFrame;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        prefs = getSharedPreferences(PrefsName, MODE_PRIVATE);
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        loadSettings();
        buildUi();
        showLauncher();
    }

    @Override
    protected void onDestroy() {
        releasePlayer();
        super.onDestroy();
    }

    @Override
    public boolean dispatchKeyEvent(KeyEvent event) {
        if (!playerVisible || event.getAction() != KeyEvent.ACTION_UP) {
            return super.dispatchKeyEvent(event);
        }

        int keyCode = event.getKeyCode();
        if (keyCode == KeyEvent.KEYCODE_SPACE) {
            togglePause();
            return true;
        }

        if (keyCode == KeyEvent.KEYCODE_BACK ||
            keyCode == KeyEvent.KEYCODE_ESCAPE ||
            keyCode == KeyEvent.KEYCODE_F11) {
            showLauncher();
            return true;
        }

        return true;
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode != RequestVideos || resultCode != RESULT_OK || data == null) {
            return;
        }

        int flags = data.getFlags() & (Intent.FLAG_GRANT_READ_URI_PERMISSION | Intent.FLAG_GRANT_WRITE_URI_PERMISSION);
        Set<String> existing = new HashSet<>(playlist);
        int added = 0;

        ClipData clipData = data.getClipData();
        if (clipData != null) {
            for (int i = 0; i < clipData.getItemCount(); i++) {
                Uri uri = clipData.getItemAt(i).getUri();
                added += addVideoUri(uri, flags, existing);
            }
        } else if (data.getData() != null) {
            added += addVideoUri(data.getData(), flags, existing);
        }

        saveSettings();
        refreshPlaylist();
        toast(added + " video eklendi.");
    }

    private int addVideoUri(Uri uri, int flags, Set<String> existing) {
        if (uri == null) {
            return 0;
        }

        try {
            getContentResolver().takePersistableUriPermission(uri, flags & Intent.FLAG_GRANT_READ_URI_PERMISSION);
        } catch (RuntimeException ignored) {
            // Some providers do not grant persistable permissions.
        }

        String value = uri.toString();
        if (!existing.add(value)) {
            return 0;
        }

        playlist.add(value);
        return 1;
    }

    private void buildUi() {
        root = new FrameLayout(this);
        root.setBackgroundColor(BackgroundColor);
        setContentView(root);

        launcherView = new LinearLayout(this);
        launcherView.setOrientation(LinearLayout.VERTICAL);
        launcherView.setPadding(dp(28), dp(24), dp(28), dp(24));
        launcherView.setBackgroundColor(BackgroundColor);
        root.addView(launcherView, new FrameLayout.LayoutParams(-1, -1));

        buildHeader();
        buildLauncherContent();

        playerView = new FrameLayout(this);
        playerView.setBackgroundColor(Color.BLACK);
        standbyVideoLayout = new VLCVideoLayout(this);
        activeVideoLayout = new VLCVideoLayout(this);
        playerView.addView(standbyVideoLayout, new FrameLayout.LayoutParams(-1, -1));
        playerView.addView(activeVideoLayout, new FrameLayout.LayoutParams(-1, -1));
        root.addView(playerView, new FrameLayout.LayoutParams(-1, -1));
        playerView.setVisibility(View.GONE);
    }

    private void buildHeader() {
        LinearLayout header = new LinearLayout(this);
        header.setGravity(Gravity.CENTER_VERTICAL);
        header.setOrientation(LinearLayout.HORIZONTAL);
        header.setPadding(0, 0, 0, dp(22));
        launcherView.addView(header, new LinearLayout.LayoutParams(-1, dp(142)));

        ImageView logo = new ImageView(this);
        logo.setImageResource(getResources().getIdentifier("brand_model", "drawable", getPackageName()));
        logo.setScaleType(ImageView.ScaleType.FIT_CENTER);
        header.addView(logo, new LinearLayout.LayoutParams(dp(132), dp(132)));

        LinearLayout titleArea = new LinearLayout(this);
        titleArea.setOrientation(LinearLayout.VERTICAL);
        titleArea.setGravity(Gravity.CENTER_VERTICAL);
        titleArea.setPadding(dp(22), 0, dp(16), 0);
        header.addView(titleArea, new LinearLayout.LayoutParams(0, -1, 1));

        TextView title = text("VideoWallPlayer", 34, Color.WHITE, true);
        titleArea.addView(title);
        TextView subtitle = text("Kenar, kontrol cubugu ve metin olmadan tam ekran video wall oynatma", 15, Color.rgb(161, 237, 240), false);
        titleArea.addView(subtitle);
    }

    private void buildLauncherContent() {
        LinearLayout columns = new LinearLayout(this);
        columns.setOrientation(LinearLayout.HORIZONTAL);
        launcherView.addView(columns, new LinearLayout.LayoutParams(-1, 0, 1));

        LinearLayout playlistPanel = panel();
        columns.addView(playlistPanel, new LinearLayout.LayoutParams(0, -1, 1.55f));

        Space(columns, 20, 1);

        ScrollView settingsScroll = new ScrollView(this);
        LinearLayout settingsPanel = panel();
        settingsScroll.addView(settingsPanel);
        columns.addView(settingsScroll, new LinearLayout.LayoutParams(0, -1, 1));

        playlistPanel.addView(text("Oynatma Listesi", 24, Color.WHITE, true));

        playlistAdapter = new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1, displayPlaylist()) {
            @Override
            public View getView(int position, View convertView, android.view.ViewGroup parent) {
                TextView row = (TextView) super.getView(position, convertView, parent);
                row.setTextColor(Color.WHITE);
                row.setTextSize(15);
                row.setSingleLine(true);
                row.setPadding(dp(14), dp(10), dp(14), dp(10));
                row.setBackgroundColor(position == playlistListView.getCheckedItemPosition()
                    ? Color.rgb(20, 58, 68)
                    : SurfaceAltColor);
                return row;
            }
        };
        playlistListView = new ListView(this);
        playlistListView.setAdapter(playlistAdapter);
        playlistListView.setBackground(rounded(SurfaceAltColor, 8, Color.rgb(48, 60, 78)));
        playlistListView.setDividerHeight(dp(6));
        playlistListView.setCacheColorHint(Color.TRANSPARENT);
        playlistListView.setChoiceMode(ListView.CHOICE_MODE_SINGLE);
        playlistListView.setOnItemClickListener((parent, view, position, id) -> playlistAdapter.notifyDataSetChanged());
        LinearLayout.LayoutParams listParams = new LinearLayout.LayoutParams(-1, 0, 1);
        listParams.setMargins(0, dp(12), 0, 0);
        playlistPanel.addView(playlistListView, listParams);

        LinearLayout playlistButtons = new LinearLayout(this);
        playlistButtons.setOrientation(LinearLayout.HORIZONTAL);
        playlistButtons.setPadding(0, dp(14), 0, 0);
        playlistPanel.addView(playlistButtons);

        playlistButtons.addView(button("Video Ekle", v -> selectVideos(), false));
        Space(playlistButtons, 8, 0);
        playlistButtons.addView(button("Secileni Sil", v -> removeSelected(), false));
        Space(playlistButtons, 8, 0);
        playlistButtons.addView(button("Temizle", v -> clearPlaylist(), false));

        statusLabel = text("", 13, Color.rgb(172, 187, 205), false);
        statusLabel.setPadding(0, dp(12), 0, 0);
        playlistPanel.addView(statusLabel);

        settingsPanel.addView(text("Ayarlar", 24, Color.WHITE, true));

        LinearLayout actionRow = new LinearLayout(this);
        actionRow.setOrientation(LinearLayout.HORIZONTAL);
        actionRow.setPadding(0, dp(10), 0, dp(18));
        settingsPanel.addView(actionRow);
        actionRow.addView(button("Videoyu Baslat", v -> startPlayback(), true));
        Space(actionRow, 8, 0);
        actionRow.addView(button("Ayar Kaydet", v -> {
            saveSettings();
            toast("Ayarlar kaydedildi.");
        }, false));

        repeatSpinner = spinner(new String[] {"Liste bitince basa don", "Ayni videoyu tekrarla", "Liste bitince dur"});
        hardwareSpinner = spinner(new String[] {"Otomatik", "Kapali"});
        shuffleCheckBox = checkbox("Karisik oynat");
        mutedCheckBox = checkbox("Sessiz");
        cachePicker = new NumberPicker(this);
        cachePicker.setMinValue(100);
        cachePicker.setMaxValue(10000);
        cachePicker.setValue(Math.max(100, prefs.getInt(KeyCache, 3000)));
        cachePicker.setBackground(rounded(SurfaceAltColor, 6, Color.rgb(58, 70, 88)));

        settingsPanel.addView(field("Tekrar modu", repeatSpinner));
        LinearLayout optionRow = new LinearLayout(this);
        optionRow.setOrientation(LinearLayout.HORIZONTAL);
        optionRow.setPadding(0, dp(8), 0, dp(4));
        optionRow.addView(shuffleCheckBox);
        Space(optionRow, 16, 0);
        optionRow.addView(mutedCheckBox);
        settingsPanel.addView(optionRow);
        settingsPanel.addView(field("Donanim hizlandirma", hardwareSpinner));
        settingsPanel.addView(field("Onbellek (ms)", cachePicker));
        TextView help = text("Onbellek sonraki videoyu hazirlamak icin kullanilir; ilk acilis dusuk gecikmeli baslar. Video ekraninda Space duraklatir/devam ettirir. Geri, Esc veya F11 oynatimdan cikar.", 13, TextMutedColor, false);
        help.setPadding(0, dp(12), 0, 0);
        settingsPanel.addView(help);

        repeatSpinner.setSelection(prefs.getInt(KeyRepeat, RepeatAll));
        hardwareSpinner.setSelection(prefs.getInt(KeyHardware, 0));
        shuffleCheckBox.setChecked(prefs.getBoolean(KeyShuffle, false));
        mutedCheckBox.setChecked(prefs.getBoolean(KeyMuted, false));
        refreshPlaylist();
    }

    private LinearLayout panel() {
        LinearLayout panel = new LinearLayout(this);
        panel.setOrientation(LinearLayout.VERTICAL);
        panel.setPadding(dp(22), dp(20), dp(22), dp(20));
        panel.setBackground(rounded(SurfaceColor, 10, Color.rgb(38, 48, 64)));
        return panel;
    }

    private View field(String label, View editor) {
        LinearLayout box = new LinearLayout(this);
        box.setOrientation(LinearLayout.VERTICAL);
        box.setPadding(0, dp(12), 0, dp(8));
        box.addView(text(label, 13, TextMutedColor, false));
        box.addView(editor, new LinearLayout.LayoutParams(-1, -2));
        return box;
    }

    private Spinner spinner(String[] values) {
        Spinner spinner = new Spinner(this);
        ArrayAdapter<String> adapter = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, values);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinner.setAdapter(adapter);
        spinner.setBackground(rounded(SurfaceAltColor, 6, Color.rgb(58, 70, 88)));
        spinner.setPadding(dp(8), 0, dp(8), 0);
        return spinner;
    }

    private CheckBox checkbox(String label) {
        CheckBox checkBox = new CheckBox(this);
        checkBox.setText(label);
        checkBox.setTextColor(Color.WHITE);
        checkBox.setTextSize(14);
        return checkBox;
    }

    private Button button(String label, View.OnClickListener listener, boolean primary) {
        Button button = new Button(this);
        button.setText(label);
        button.setAllCaps(false);
        button.setTextColor(primary ? Color.rgb(4, 19, 27) : Color.WHITE);
        button.setTypeface(Typeface.DEFAULT_BOLD);
        button.setTextSize(13);
        button.setBackground(rounded(primary ? AccentColor : Color.rgb(38, 48, 64), 8, primary ? AccentHoverColor : Color.rgb(58, 70, 88)));
        button.setMinHeight(dp(40));
        button.setMinimumHeight(dp(40));
        button.setMinWidth(primary ? dp(152) : dp(112));
        button.setPadding(dp(14), 0, dp(14), 0);
        button.setOnClickListener(listener);
        return button;
    }

    private TextView text(String value, int sp, int color, boolean bold) {
        TextView textView = new TextView(this);
        textView.setText(value);
        textView.setTextColor(color);
        textView.setTextSize(sp);
        if (bold) {
            textView.setTypeface(Typeface.DEFAULT_BOLD);
        }
        return textView;
    }

    private GradientDrawable rounded(int color, int radiusDp, int strokeColor) {
        GradientDrawable drawable = new GradientDrawable();
        drawable.setColor(color);
        drawable.setCornerRadius(dp(radiusDp));
        drawable.setStroke(dp(1), strokeColor);
        return drawable;
    }

    private void Space(LinearLayout parent, int sizeDp, int weight) {
        View spacer = new View(this);
        LinearLayout.LayoutParams params = parent.getOrientation() == LinearLayout.HORIZONTAL
            ? new LinearLayout.LayoutParams(weight == 0 ? dp(sizeDp) : 0, -1, weight)
            : new LinearLayout.LayoutParams(-1, dp(sizeDp));
        parent.addView(spacer, params);
    }

    private void selectVideos() {
        Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
        intent.addCategory(Intent.CATEGORY_OPENABLE);
        intent.setType("video/*");
        intent.putExtra(Intent.EXTRA_ALLOW_MULTIPLE, true);
        intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION | Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION);
        startActivityForResult(intent, RequestVideos);
    }

    private void removeSelected() {
        int position = playlistListView.getCheckedItemPosition();
        if (position == AdapterView.INVALID_POSITION || position >= playlist.size()) {
            return;
        }

        playlist.remove(position);
        saveSettings();
        refreshPlaylist();
    }

    private void clearPlaylist() {
        playlist.clear();
        saveSettings();
        refreshPlaylist();
    }

    private void startPlayback() {
        saveSettings();
        if (playlist.isEmpty()) {
            toast("Oynatma listesi bos.");
            return;
        }

        buildPlayOrder();
        orderIndex = 0;
        showPlayer();
        playCurrent();
    }

    private void buildPlayOrder() {
        playOrder.clear();
        for (int i = 0; i < playlist.size(); i++) {
            playOrder.add(i);
        }
        if (shuffleCheckBox.isChecked() && playOrder.size() > 1) {
            Collections.shuffle(playOrder, random);
        }
    }

    private void showPlayer() {
        launcherView.setVisibility(View.GONE);
        playerView.setVisibility(View.VISIBLE);
        playerVisible = true;
        hideSystemUi();
        ensurePlayer();
    }

    private void showLauncher() {
        playerVisible = false;
        paused = false;
        releasePlayer();
        playerView.setVisibility(View.GONE);
        launcherView.setVisibility(View.VISIBLE);
        showSystemUi();
    }

    private void ensurePlayer() {
        if (libVlc != null && activePlayer != null && standbyPlayer != null) {
            return;
        }

        ArrayList<String> options = new ArrayList<>();
        options.add("--no-video-title-show");
        options.add("--no-osd");
        options.add("--quiet");
        options.add("--file-caching=" + startupCachingMs());
        options.add("--network-caching=" + startupCachingMs());
        options.add("--avcodec-hw=" + (hardwareSpinner.getSelectedItemPosition() == 1 ? "none" : "any"));

        libVlc = new LibVLC(this, options);
        activePlayer = createPlayer(activeVideoLayout);
        standbyPlayer = createPlayer(standbyVideoLayout);
    }

    private MediaPlayer createPlayer(VLCVideoLayout layout) {
        MediaPlayer player = new MediaPlayer(libVlc);
        player.attachViews(layout, null, false, false);
        player.setEventListener(event -> handlePlayerEvent(player, event));
        player.setVolume(mutedCheckBox.isChecked() ? 0 : 100);
        return player;
    }

    private void handlePlayerEvent(MediaPlayer player, MediaPlayer.Event event) {
        if (event.type == MediaPlayer.Event.EndReached) {
            if (player == activePlayer) {
                runOnUiThread(this::playNextAfterEnd);
            }
            return;
        }

        if (event.type == MediaPlayer.Event.EncounteredError) {
            if (player == activePlayer) {
                runOnUiThread(this::handlePlaybackError);
            }
            return;
        }

        if (event.type == MediaPlayer.Event.Playing && player == standbyPlayer && standbyPriming) {
            mainHandler.removeCallbacks(freezeStandbyRunnable);
            mainHandler.postDelayed(freezeStandbyRunnable, StandbyFirstFramePauseMs);
        }
    }

    private void playCurrent() {
        if (activePlayer == null || playOrder.isEmpty() || orderIndex < 0 || orderIndex >= playOrder.size()) {
            return;
        }

        Uri uri = Uri.parse(playlist.get(playOrder.get(orderIndex)));
        try {
            activePlayer.stop();
            closeActiveMediaDescriptor();
            Media media = createMedia(uri, true, startupCachingMs());
            boolean hardware = hardwareSpinner.getSelectedItemPosition() != 1;
            media.setHWDecoderEnabled(hardware, false);
            activePlayer.setMedia(media);
            media.release();
            activeVideoLayout.bringToFront();
            activePlayer.setVolume(mutedCheckBox.isChecked() ? 0 : 100);
            activePlayer.play();
            paused = false;
            hideSystemUi();
            prepareNextIfNeeded();
        } catch (Exception ex) {
            statusLabel.setText("Video acilamadi: " + displayName(uri));
            handlePlaybackError();
        }
    }

    private Media createMedia(Uri uri, boolean active, int cachingMs) throws Exception {
        Media media;
        if (ContentResolver.SCHEME_CONTENT.equalsIgnoreCase(uri.getScheme())) {
            AssetFileDescriptor descriptor = getContentResolver().openAssetFileDescriptor(uri, "r");
            if (descriptor == null) {
                throw new IllegalStateException("Dosya izni alinamadi.");
            }

            if (active) {
                activeMediaDescriptor = descriptor;
            } else {
                standbyMediaDescriptor = descriptor;
            }
            media = new Media(libVlc, descriptor);
        } else if (ContentResolver.SCHEME_FILE.equalsIgnoreCase(uri.getScheme()) && uri.getPath() != null) {
            media = new Media(libVlc, uri.getPath());
        } else {
            media = new Media(libVlc, uri);
        }

        media.addOption(":file-caching=" + cachingMs);
        media.addOption(":network-caching=" + cachingMs);
        media.addOption(":avcodec-hw=" + (hardwareSpinner.getSelectedItemPosition() == 1 ? "none" : "any"));
        return media;
    }

    private void handlePlaybackError() {
        if (!playerVisible || playOrder.isEmpty() || orderIndex < 0 || orderIndex >= playOrder.size()) {
            return;
        }

        Uri uri = Uri.parse(playlist.get(playOrder.get(orderIndex)));
        statusLabel.setText("VLC bu videoyu oynatamadi: " + displayName(uri));
        if (playOrder.size() <= 1) {
            toast("Video oynatilamadi.");
            showLauncher();
            return;
        }

        toast("Video oynatilamadi, siradaki deneniyor.");
        orderIndex = (orderIndex + 1) % playOrder.size();
        clearStandby();
        playCurrent();
    }

    private void playNextAfterEnd() {
        if (!playerVisible) {
            return;
        }

        Integer next = nextOrderIndex();
        if (next == null) {
            showLauncher();
            return;
        }

        orderIndex = next;
        if (!swapToStandbyIfAvailable()) {
            playCurrent();
        }
    }

    private void togglePause() {
        if (activePlayer == null || !playerVisible) {
            return;
        }
        paused = !paused;
        if (paused) {
            activePlayer.pause();
        } else {
            activePlayer.play();
        }
        hideSystemUi();
    }

    private void prepareNextIfNeeded() {
        if (!playerVisible || activePlayer == null || standbyPlayer == null || playOrder.isEmpty()) {
            return;
        }

        Integer next = nextOrderIndex();
        if (next == null || (standbyPlaying && standbyOrderIndex == next)) {
            return;
        }

        preloadStandby(next);
    }

    private Integer nextOrderIndex() {
        if (playOrder.isEmpty()) {
            return null;
        }

        if (repeatSpinner.getSelectedItemPosition() == RepeatOne) {
            return orderIndex;
        }

        int next = orderIndex + 1;
        if (next < playOrder.size()) {
            return next;
        }

        if (repeatSpinner.getSelectedItemPosition() == RepeatNone) {
            return null;
        }

        return 0;
    }

    private void preloadStandby(int nextOrderIndex) {
        if (nextOrderIndex < 0 || nextOrderIndex >= playOrder.size()) {
            return;
        }

        clearStandby();
        Uri uri = Uri.parse(playlist.get(playOrder.get(nextOrderIndex)));
        try {
            Media media = createMedia(uri, false, standbyCachingMs());
            boolean hardware = hardwareSpinner.getSelectedItemPosition() != 1;
            media.setHWDecoderEnabled(hardware, false);
            standbyPlayer.setMedia(media);
            media.release();
            standbyOrderIndex = nextOrderIndex;
            standbyPriming = true;
            standbyReady = false;
            standbyPlaying = true;
            standbyPlayer.setVolume(0);
            standbyVideoLayout.bringToFront();
            activeVideoLayout.bringToFront();
            standbyPlayer.play();
        } catch (Exception ex) {
            clearStandby();
        }
    }

    private void freezeStandbyOnFirstFrame() {
        if (!playerVisible || !standbyPriming || !standbyPlaying || standbyPlayer == null) {
            return;
        }

        try {
            standbyPlayer.pause();
            if (standbyPlayer.getTime() > 0) {
                standbyPlayer.setTime(0);
            }
        } catch (RuntimeException ignored) {
        }

        standbyPriming = false;
        standbyReady = true;
    }

    private boolean swapToStandbyIfAvailable() {
        if (!standbyPlaying || standbyPlayer == null || standbyOrderIndex != orderIndex) {
            return false;
        }

        mainHandler.removeCallbacks(freezeStandbyRunnable);
        boolean wasReady = standbyReady;
        MediaPlayer previousPlayer = activePlayer;
        VLCVideoLayout previousLayout = activeVideoLayout;
        AssetFileDescriptor previousDescriptor = activeMediaDescriptor;

        activePlayer = standbyPlayer;
        activeVideoLayout = standbyVideoLayout;
        activeMediaDescriptor = standbyMediaDescriptor;

        standbyPlayer = previousPlayer;
        standbyVideoLayout = previousLayout;
        standbyMediaDescriptor = null;
        standbyOrderIndex = -1;
        standbyPlaying = false;
        standbyPriming = false;
        standbyReady = false;

        activeVideoLayout.bringToFront();
        activePlayer.setVolume(mutedCheckBox.isChecked() ? 0 : 100);
        if (wasReady) {
            try {
                activePlayer.setTime(0);
            } catch (RuntimeException ignored) {
            }
        }
        activePlayer.play();

        standbyPlayer.stop();
        closeDescriptor(previousDescriptor);
        paused = false;
        prepareNextIfNeeded();
        return true;
    }

    private int startupCachingMs() {
        return Math.max(100, Math.min(cachePicker.getValue(), StartupCachingMs));
    }

    private int standbyCachingMs() {
        return Math.max(100, Math.min(cachePicker.getValue(), 30000));
    }

    private void releasePlayer() {
        mainHandler.removeCallbacks(freezeStandbyRunnable);

        if (activePlayer != null) {
            activePlayer.stop();
            activePlayer.detachViews();
            activePlayer.release();
            activePlayer = null;
        }

        if (standbyPlayer != null) {
            standbyPlayer.stop();
            standbyPlayer.detachViews();
            standbyPlayer.release();
            standbyPlayer = null;
        }

        closeActiveMediaDescriptor();
        closeStandbyMediaDescriptor();
        standbyOrderIndex = -1;
        standbyPlaying = false;
        standbyPriming = false;
        standbyReady = false;

        if (libVlc != null) {
            libVlc.release();
            libVlc = null;
        }
    }

    private void clearStandby() {
        mainHandler.removeCallbacks(freezeStandbyRunnable);
        if (standbyPlayer != null) {
            standbyPlayer.stop();
        }
        closeStandbyMediaDescriptor();
        standbyOrderIndex = -1;
        standbyPlaying = false;
        standbyPriming = false;
        standbyReady = false;
    }

    private void closeActiveMediaDescriptor() {
        closeDescriptor(activeMediaDescriptor);
        activeMediaDescriptor = null;
    }

    private void closeStandbyMediaDescriptor() {
        closeDescriptor(standbyMediaDescriptor);
        standbyMediaDescriptor = null;
    }

    private void closeDescriptor(AssetFileDescriptor descriptor) {
        if (descriptor == null) {
            return;
        }

        try {
            descriptor.close();
        } catch (Exception ignored) {
        }
    }

    private void hideSystemUi() {
        getWindow().getDecorView().setSystemUiVisibility(
            View.SYSTEM_UI_FLAG_FULLSCREEN |
            View.SYSTEM_UI_FLAG_HIDE_NAVIGATION |
            View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY |
            View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN |
            View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION |
            View.SYSTEM_UI_FLAG_LAYOUT_STABLE);
        if (android.os.Build.VERSION.SDK_INT >= 30) {
            WindowInsetsController controller = getWindow().getInsetsController();
            if (controller != null) {
                controller.hide(WindowInsets.Type.statusBars() | WindowInsets.Type.navigationBars());
            }
        }
    }

    private void showSystemUi() {
        getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LAYOUT_STABLE);
        if (android.os.Build.VERSION.SDK_INT >= 30) {
            WindowInsetsController controller = getWindow().getInsetsController();
            if (controller != null) {
                controller.show(WindowInsets.Type.statusBars() | WindowInsets.Type.navigationBars());
            }
        }
    }

    private void loadSettings() {
        playlist.clear();
        String json = prefs.getString(KeyPlaylist, "[]");
        try {
            JSONArray array = new JSONArray(json);
            for (int i = 0; i < array.length(); i++) {
                playlist.add(array.getString(i));
            }
        } catch (JSONException ignored) {
            playlist.clear();
        }
    }

    private void saveSettings() {
        JSONArray array = new JSONArray();
        for (String item : playlist) {
            array.put(item);
        }

        prefs.edit()
            .putString(KeyPlaylist, array.toString())
            .putInt(KeyRepeat, repeatSpinner == null ? RepeatAll : repeatSpinner.getSelectedItemPosition())
            .putBoolean(KeyShuffle, shuffleCheckBox != null && shuffleCheckBox.isChecked())
            .putBoolean(KeyMuted, mutedCheckBox != null && mutedCheckBox.isChecked())
            .putInt(KeyHardware, hardwareSpinner == null ? 0 : hardwareSpinner.getSelectedItemPosition())
            .putInt(KeyCache, cachePicker == null ? 3000 : cachePicker.getValue())
            .apply();
    }

    private void refreshPlaylist() {
        if (playlistAdapter != null) {
            playlistAdapter.clear();
            playlistAdapter.addAll(displayPlaylist());
            playlistAdapter.notifyDataSetChanged();
        }

        if (statusLabel != null) {
            statusLabel.setText(playlist.size() + " video listede.");
        }
    }

    private List<String> displayPlaylist() {
        ArrayList<String> display = new ArrayList<>();
        for (String value : playlist) {
            display.add(displayName(Uri.parse(value)));
        }
        return display;
    }

    private String displayName(Uri uri) {
        if (ContentResolver.SCHEME_CONTENT.equalsIgnoreCase(uri.getScheme())) {
            try (Cursor cursor = getContentResolver().query(uri, new String[] {OpenableColumns.DISPLAY_NAME}, null, null, null)) {
                if (cursor != null && cursor.moveToFirst()) {
                    int index = cursor.getColumnIndex(OpenableColumns.DISPLAY_NAME);
                    if (index >= 0) {
                        String value = cursor.getString(index);
                        if (value != null && !value.isEmpty()) {
                            return value;
                        }
                    }
                }
            } catch (RuntimeException ignored) {
            }
        }

        String label = uri.getLastPathSegment();
        return label == null || label.isEmpty() ? uri.toString() : label;
    }

    private int dp(int value) {
        return Math.round(value * getResources().getDisplayMetrics().density);
    }

    private void toast(String message) {
        Toast.makeText(this, message, Toast.LENGTH_SHORT).show();
    }
}
